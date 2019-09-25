using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Useful for processing 3D rotations. Riemer's XNA tutorials have a nice introductory
	///   example of use: http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2/Quaternions.php
	///   http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2/Flight_kinematics.php
	/// </summary>
	[DebuggerDisplay("Quaternion(X={X}, Y={Y}, Z={Z}, W={W})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Quaternion : IEquatable<Quaternion>, LerpWithParent<Quaternion>
	{
		public Quaternion(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

		public Quaternion(string quaternionAsString)
		{
			var values = quaternionAsString.SplitIntoFloats();
			if (values.Length != 4)
				throw new InvalidNumberOfDatatypeComponents<Quaternion>(quaternionAsString);
			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}

		[Pure] public float X { get; }
		[Pure] public float Y { get; }
		[Pure] public float Z { get; }
		[Pure] public float W { get; }
		[Pure] public float Length => MathExtensions.Sqrt(X * X + Y * Y + Z * Z + W * W);

		[Pure]
		public Quaternion Normalize()
		{
			return this * (1.0f / Length);
		}

		public static Quaternion CreateFromAxisAngle(Vector3D axis, float angle)
		{
			var vectorPart = MathExtensions.Sin(angle * 0.5f) * axis;
			return new Quaternion(vectorPart.X, vectorPart.Y, vectorPart.Z,
				MathExtensions.Cos(angle * 0.5f));
		}

		[Pure]
		public static Quaternion operator *(Quaternion q, float f)
		{
			return new Quaternion(q.X * f, q.Y * f, q.Z * f, q.W * f);
		}

		/// <summary>
		///   http://molecularmusings.wordpress.com/2013/05/24/a-faster-quaternion-vector-multiplication/
		/// </summary>
		[Pure]
		public static Vector3D operator *(Quaternion q, Vector3D v)
		{
			var qv = q.Vector3D;
			var t = 2.0f * qv.Cross(v);
			return v + q.W * t + qv.Cross(t);
		}

		[Pure] public Vector3D Vector3D => new Vector3D(X, Y, Z);

		/// <summary>
		///   http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/code/
		/// </summary>
		[Pure]
		public static Quaternion operator *(Quaternion q1, Quaternion q2)
		{
			return new Quaternion(q2.X * q1.W + q2.Y * q1.Z - q2.Z * q1.Y + q2.W * q1.X,
				-q2.X * q1.Z + q2.Y * q1.W + q2.Z * q1.X + q2.W * q1.Y,
				q2.X * q1.Y - q2.Y * q1.X + q2.Z * q1.W + q2.W * q1.Z,
				-q2.X * q1.X - q2.Y * q1.Y - q2.Z * q1.Z + q2.W * q1.W);
		}

		[Pure]
		public Quaternion Conjugate()
		{
			return new Quaternion(-X, -Y, -Z, W);
		}

		public static Quaternion CreateLookAt(Vector3D eyePosition, Vector3D targetPosition,
			Vector3D cameraUp)
		{
			var matLookAt = Matrix.CreateLookAt(eyePosition, targetPosition, cameraUp);
			return CreateFromRotationMatrix(matLookAt);
		}

		public static Quaternion CreateFromRotationMatrix(Matrix m)
		{
			if (m[0] + m[5] + m[10] > 0.0f)
				return CreateFromNormalRotationMatrix(m);
			if (m[0] > m[5] && m[0] > m[10])
				return CreateFromXRotationMatrix(m);
			return m[5] > m[10] ? CreateFromYRotationMatrix(m) : CreateFromZRotationMatrix(m);
		}

		private static Quaternion CreateFromNormalRotationMatrix(Matrix m)
		{
			var t = 1.0f + m[0] + m[5] + m[10];
			var s = 0.5f * t.InvSqrt();
			return new Quaternion(m[6] - m[9], m[8] - m[2], m[1] - m[4], t) * s;
		}

		private static Quaternion CreateFromXRotationMatrix(Matrix m)
		{
			var t = 1.0f + m[0] - m[5] - m[10];
			var s = 0.5f * t.InvSqrt();
			return new Quaternion(t, m[1] + m[4], m[8] + m[2], m[6] - m[9]) * s;
		}

		private static Quaternion CreateFromYRotationMatrix(Matrix m)
		{
			var t = 1.0f - m[0] + m[5] - m[10];
			var s = 0.5f * t.InvSqrt();
			return new Quaternion(m[1] + m[4], t, m[6] + m[9], m[8] - m[2]) * s;
		}

		private static Quaternion CreateFromZRotationMatrix(Matrix m)
		{
			var t = 1.0f - m[0] - m[5] + m[10];
			var s = 0.5f * t.InvSqrt();
			return new Quaternion(m[8] + m[2], m[6] + m[9], t, m[1] - m[4]) * s;
		}

		[Pure]
		public Quaternion Lerp(Quaternion other, float interpolation)
		{
			return new Quaternion(X.Lerp(other.X, interpolation), Y.Lerp(other.Y, interpolation),
				Z.Lerp(other.Z, interpolation), W.Lerp(other.W, interpolation));
		}

		[Pure]
		public Quaternion Lerp(Quaternion other, float interpolation, Quaternion parentQuaternion)
		{
			return parentQuaternion * Lerp(other, interpolation);
		}

		/// <summary>
		///   For small interpolations (like draw interpolations) Lerp is fine, but when using bigger
		///   changes Slerp is much better and handles orientation changes nicer, but it is a bit slower.
		/// </summary>
		[Pure]
		// ReSharper disable once MethodTooLong
		public Quaternion Slerp(Quaternion other, float interpolation)
		{
			var cos = Dot(other);
			var sin = MathExtensions.Sqrt((1.0f - cos * cos).Abs());
			var angle = MathExtensions.Atan2(sin, cos);
			var thisPart = this * (MathExtensions.Sin((1 - interpolation) * angle) / sin);
			var otherPart = other * (MathExtensions.Sin(interpolation * angle) / sin);
			return new Quaternion(thisPart.X + otherPart.X, thisPart.Y + otherPart.Y,
				thisPart.Z + otherPart.Z, thisPart.W + otherPart.W);
		}

		[Pure]
		// ReSharper disable once MethodNameNotMeaningful
		public float Dot(Quaternion other)
		{
			return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
		}

		[Pure]
		public bool Equals(Quaternion other)
		{
			return X.IsNearlyEqual(other.X) && Y.IsNearlyEqual(other.Y) && Z.IsNearlyEqual(other.Z) &&
			       W.IsNearlyEqual(other.W);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Quaternion ? Equals((Quaternion) other) : base.Equals(other);
		}

		[Pure]
		public static bool operator ==(Quaternion quat1, Quaternion quat2)
		{
			return quat1.Equals(quat2);
		}

		[Pure]
		public static bool operator !=(Quaternion quat1, Quaternion quat2)
		{
			return !quat1.Equals(quat2);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return (((((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Z.GetHashCode()) * 397) ^
				       W.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return X.ToInvariantString() + ", " + Y.ToInvariantString() + ", " + Z.ToInvariantString() +
			       ", " +
			       W.ToInvariantString();
		}

		/// <summary>
		///   Derived from:
		///   http://stackoverflow.com/questions/1031005/is-there-an-algorithm-for-converting-quaternion-rotations-to-euler-angle-rotatio/2070899#2070899
		///   Returns Euler angles in ZYX order (Yaw, Pitch and Roll) to match Matrix.CreateRotationZYX.
		/// </summary>
		[Pure]
		// ReSharper disable once MethodTooLong
		public EulerAngles ToEulerZYX()
		{
			var ww = W * W;
			var xx = X * X;
			var yy = Y * Y;
			var zz = Z * Z;
			var lengthSqd = xx + yy + zz + ww;
			var singularityTest = Y * W - X * Z;
			var singularityValue = Singularity * lengthSqd;
			return singularityTest > singularityValue
				? new EulerAngles(-2 * MathExtensions.Atan2(Z, W), MathExtensions.QuarterCircleDegrees,
					0.0f)
				: singularityTest < -singularityValue
					? new EulerAngles(2 * MathExtensions.Atan2(Z, W), -MathExtensions.QuarterCircleDegrees,
						0.0f)
					: new EulerAngles(MathExtensions.Atan2(2.0f * (Y * Z + X * W), 1.0f - 2.0f * (xx + yy)),
						MathExtensions.Asin(2.0f * singularityTest / lengthSqd),
						MathExtensions.Atan2(2.0f * (X * Y + Z * W), 1.0f - 2.0f * (yy + zz)));
		}

		private const float Singularity = 0.499f;

		[Pure]
		public float CalculateAxisAngle()
		{
			return (2.0f * (float) Math.Acos(W.Clamp(-1.0f, 1.0f))).RadiansToDegrees() % 360;
		}

		[Pure]
		public Vector3D CalculateRotationAxis()
		{
			return new Vector3D(X / MathExtensions.Sqrt(1 - W * W), Y / MathExtensions.Sqrt(1 - W * W),
				Z / MathExtensions.Sqrt(1 - W * W));
		}
	}
}