using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Specifies a position in 3D space, used for 3D geometry, cameras and 3D physics.
	/// </summary>
	[DebuggerDisplay("Vector3D({X}, {Y}, {Z})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Vector3D : IEquatable<Vector3D>, LerpWithParent<Vector3D>
	{
		public Vector3D(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3D(Vector2D fromVector2D, float z = 0.0f)
		{
			X = fromVector2D.X;
			Y = fromVector2D.Y;
			Z = z;
		}

		public Vector3D(string vectorAsString)
		{
			var floats = vectorAsString.SplitIntoFloats();
			if (floats.Length != 3)
				throw new InvalidNumberOfDatatypeComponents<Vector3D>();
			X = floats[0];
			Y = floats[1];
			Z = floats[2];
		}

		public static readonly Vector3D Zero;
		public static readonly Vector3D One = new Vector3D(1, 1, 1);
		public static readonly Vector3D UnitX = new Vector3D(1, 0, 0);
		public static readonly Vector3D UnitY = new Vector3D(0, 1, 0);
		public static readonly Vector3D UnitZ = new Vector3D(0, 0, 1);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3D));

		[Pure] public float X { get; }

		[Pure] public float Y { get; }

		[Pure] public float Z { get; }

		[Pure] public Vector2D XY => new Vector2D(X, Y);

		[Pure]
		// ReSharper disable once MethodNameNotMeaningful
		public float Dot(Vector3D other)
		{
			return X * other.X + Y * other.Y + Z * other.Z;
		}

		[Pure]
		public Vector3D Cross(Vector3D other)
		{
			return new Vector3D(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z,
				X * other.Y - Y * other.X);
		}

		[Pure]
		public Vector3D Normalize()
		{
			if (LengthSquared.IsNearlyEqual(0.0f) || LengthSquared.IsNearlyEqual(1.0f))
				return this;
			var distanceInverse = 1.0f / MathExtensions.Sqrt(LengthSquared);
			return new Vector3D(X * distanceInverse, Y * distanceInverse, Z * distanceInverse);
		}

		[Pure]
		public Vector3D TransformNormal(Matrix matrix)
		{
			return matrix.TransformNormal(this);
		}

		public Vector3D Lerp(Vector3D other, float interpolation)
		{
			return new Vector3D(X.Lerp(other.X, interpolation), Y.Lerp(other.Y, interpolation),
				Z.Lerp(other.Z, interpolation));
		}

		public Vector3D Lerp(Vector3D other, float interpolation, Vector3D parentVector)
		{
			return parentVector + Lerp(other, interpolation);
		}

		[Pure] public float Length => MathExtensions.Sqrt(LengthSquared);
		[Pure] public float LengthSquared => X * X + Y * Y + Z * Z;

		[Pure]
		public static Vector3D operator +(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}

		[Pure]
		public static Vector3D operator -(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		}

		[Pure]
		public static Vector3D operator *(Vector3D v, float f)
		{
			return new Vector3D(v.X * f, v.Y * f, v.Z * f);
		}

		[Pure]
		public static Vector3D operator *(float f, Vector3D v)
		{
			return v * f;
		}

		[Pure]
		public static Vector3D operator *(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		[Pure]
		public static Vector3D operator /(Vector3D v, float f)
		{
			return new Vector3D(v.X / f, v.Y / f, v.Z / f);
		}

		[Pure]
		public static Vector3D operator -(Vector3D value)
		{
			return new Vector3D(-value.X, -value.Y, -value.Z);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Vector3D ? Equals((Vector3D) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Vector3D other)
		{
			return X.IsNearlyEqual(other.X) && Y.IsNearlyEqual(other.Y) && Z.IsNearlyEqual(other.Z);
		}

		[Pure]
		public static bool operator ==(Vector3D v1, Vector3D v2)
		{
			return v1.Equals(v2);
		}

		[Pure]
		public static bool operator !=(Vector3D v1, Vector3D v2)
		{
			return !v1.Equals(v2);
		}

		[Pure]
		public static implicit operator Vector3D(Vector2D vector2D)
		{
			return new Vector3D(vector2D.X, vector2D.Y, 0);
		}

		[Pure]
		public float DistanceTo(Vector3D vector)
		{
			return (vector - this).Length;
		}

		[Pure]
		public float DistanceToSquared(Vector3D vector)
		{
			return (vector - this).LengthSquared;
		}

		[Pure]
		public Vector3D TransformTranspose(Matrix matrix)
		{
			return new Vector3D(X * matrix[0] + Y * matrix[1] + Z * matrix[2],
				X * matrix[4] + Y * matrix[5] + Z * matrix[6],
				X * matrix[8] + Y * matrix[9] + Z * matrix[10]);
		}

		[Pure]
		public Vector3D RotateAround(Vector3D axis, float angle)
		{
			var quaternion = Quaternion.CreateFromAxisAngle(axis, 0).Conjugate();
			var worldSpaceVector = Transform(quaternion);
			quaternion = Quaternion.CreateFromAxisAngle(axis, angle);
			return worldSpaceVector.Transform(quaternion);
		}

		[Pure]
		// ReSharper disable once MethodTooLong
		public Vector3D Transform(Quaternion quaternion)
		{
			var x2 = quaternion.X + quaternion.X;
			var y2 = quaternion.Y + quaternion.Y;
			var z2 = quaternion.Z + quaternion.Z;
			var xx2 = quaternion.X * x2;
			var xy2 = quaternion.X * y2;
			var xz2 = quaternion.X * z2;
			var yy2 = quaternion.Y * y2;
			var yz2 = quaternion.Y * z2;
			var zz2 = quaternion.Z * z2;
			var wx2 = quaternion.W * x2;
			var wy2 = quaternion.W * y2;
			var wz2 = quaternion.W * z2;
			return new Vector3D(X * (1.0f - yy2 - zz2) + Y * (xy2 - wz2) + Z * (xz2 + wy2),
				X * (xy2 + wz2) + Y * (1.0f - xx2 - zz2) + Z * (yz2 - wx2),
				X * (xz2 - wy2) + Y * (yz2 + wx2) + Z * (1.0f - xx2 - yy2));
		}

		[Pure]
		public Vector3D Reflect(Vector3D planeNormal)
		{
			return this - planeNormal * Dot(planeNormal) * 2;
		}

		[Pure]
		public Vector3D IntersectNormal(Vector3D normal)
		{
			return normal * Dot(normal);
		}

		[Pure]
		public Vector3D IntersectRay(Vector3D rayOrigin, Vector3D rayDirection)
		{
			return rayDirection * (this - rayOrigin).Dot(rayDirection) + rayOrigin;
		}

		[Pure]
		public Vector3D IntersectPlane(Vector3D planeNormal)
		{
			return this - planeNormal * Dot(planeNormal);
		}

		/// <summary>
		///   Gets angle in between this vector and other vector.
		/// </summary>
		[Pure]
		public float Angle(Vector3D other)
		{
			var dotProduct = Dot(other);
			var cosine = dotProduct / (Length * other.Length);
			if (cosine >= 1.0f)
				return 0;
			if (cosine <= -1.0f)
				return MathExtensions.HalfCircleDegrees;
			return MathExtensions.Acos(cosine);
		}

		/// <summary>
		///   Gets the shortest arc quaternion to rotate this vector to the destination vector.
		/// </summary>
		[Pure]
		public Quaternion GetShortestRotation(Vector3D destination)
		{
			var normalized = Normalize();
			destination = destination.Normalize();
			var dotProduct = normalized.Dot(destination);
			if (dotProduct >= 1.0f)
				return Quaternion.Identity;
			var inverseSquare = ((1 + dotProduct) * 2).InvSqrt();
			var axis = normalized.Cross(destination);
			if (axis.Length < .001f)
				return Quaternion.CreateFromAxisAngle(UnitZ, MathExtensions.HalfCircleDegrees);
			// ReSharper disable once MaximumChainedReferences
			return new Quaternion(axis.X * inverseSquare, axis.Y * inverseSquare, axis.Z * inverseSquare,
				0.5f / inverseSquare).Normalize();
		}

		/// <summary>
		///   Interpolates two location values with tangents along a cubic Hermite spline.
		///   http://en.wikipedia.org/wiki/Cubic_Hermite_spline
		/// </summary>
		[Pure]
		public Vector3D Hermite(Vector3D tangent1, Vector3D value2, Vector3D tangent2,
			float interpolationAmount)
		{
			var weightSquared = interpolationAmount * interpolationAmount;
			var weightCubed = interpolationAmount * weightSquared;
			var blend1 = 2 * weightCubed - 3 * weightSquared + 1;
			var tangent1Blend = weightCubed - 2 * weightSquared + interpolationAmount;
			var blend2 = -2 * weightCubed + 3 * weightSquared;
			var tangent2Blend = weightCubed - weightSquared;
			return new Vector3D(
				X * blend1 + value2.X * blend2 + tangent1.X * tangent1Blend + tangent2.X * tangent2Blend,
				Y * blend1 + value2.Y * blend2 + tangent1.Y * tangent1Blend + tangent2.Y * tangent2Blend,
				Z * blend1 + value2.Z * blend2 + tangent1.Z * tangent1Blend + tangent2.Z * tangent2Blend);
		}

		/// <summary>
		///   Interpolates value2 and value3 with values before and after smoothly via Catmull-Rom spline.
		///   http://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
		/// </summary>
		[Pure]
		// ReSharper disable once MethodTooLong
		public Vector3D CatmullRom(Vector3D value2, Vector3D value3, Vector3D value4,
			float interpolationAmount)
		{
			var amountSquared = interpolationAmount * interpolationAmount;
			var amountCubic = amountSquared * interpolationAmount;
			// ReSharper disable ComplexConditionExpression
			var xValue = 0.5f *
			             (2 * value2.X + (-X + value3.X) * interpolationAmount +
			              (2 * X - 5 * value2.X + 4 * value3.X - value4.X) * amountSquared +
			              (-X + 3 * value2.X - 3 * value3.X + value4.X) * amountCubic);
			var yValue = 0.5f *
			             (2 * value2.Y + (-Y + value3.Y) * interpolationAmount +
			              (2 * Y - 5 * value2.Y + 4 * value3.Y - value4.Y) * amountSquared +
			              (-Y + 3 * value2.Y - 3 * value3.Y + value4.Y) * amountCubic);
			var zValue = 0.5f *
			             (2 * value2.Z + (-Z + value3.Z) * interpolationAmount +
			              (2 * Z - 5 * value2.Z + 4 * value3.Z - value4.Z) * amountSquared +
			              (-Z + 3 * value2.Z - 3 * value3.Z + value4.Z) * amountCubic);
			return new Vector3D(xValue, yValue, zValue);
		}

		[Pure]
		// ReSharper disable once MethodNameNotMeaningful
		public Vector3D Min(Vector3D other)
		{
			var xValue = X < other.X ? X : other.X;
			var yValue = Y < other.Y ? Y : other.Y;
			var zValue = Z < other.Z ? Z : other.Z;
			return new Vector3D(xValue, yValue, zValue);
		}

		[Pure]
		// ReSharper disable once MethodNameNotMeaningful
		public Vector3D Max(Vector3D other)
		{
			var xValue = X > other.X ? X : other.X;
			var yValue = Y > other.Y ? Y : other.Y;
			var zValue = Z > other.Z ? Z : other.Z;
			return new Vector3D(xValue, yValue, zValue);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable ImpureMethodCallOnReadonlyValueField
				return X.GetHashCode() * 23 + Y.GetHashCode() * 1023 + Z.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return X.ToInvariantString() + ", " + Y.ToInvariantString() + ", " + Z.ToInvariantString();
		}
	}
}