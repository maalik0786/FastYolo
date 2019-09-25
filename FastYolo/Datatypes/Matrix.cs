using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   4x4 Matrix with 16 floats, access happens via indexer, optimizations are done in Build Service
	///   Performance is 100x better when System.Numerics.Matrix4x4 is used instead (done automatically)
	/// </summary>
	[DebuggerDisplay(
		"Matrix(Right={Right},\nUp={Up},\nForward={Forward},\nTranslation={Translation})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Matrix : IEquatable<Matrix>
	{
		public Matrix(params float[] values)
		{
			m11 = values[0];
			m12 = values[1];
			m13 = values[2];
			m14 = values[3];
			m21 = values[4];
			m22 = values[5];
			m23 = values[6];
			m24 = values[7];
			m31 = values[8];
			m32 = values[9];
			m33 = values[10];
			m34 = values[11];
			m41 = values[12];
			m42 = values[13];
			m43 = values[14];
			m44 = values[15];
		}

		private readonly float m11,
			m12,
			m13,
			m14,
			m21,
			m22,
			m23,
			m24,
			m31,
			m32,
			m33,
			m34,
			m41,
			m42,
			m43,
			m44;

		public Matrix(string matrixString)
			: this(matrixString.SplitIntoFloats(ValueStringSeparator))
		{
		}

		private const string ValueStringSeparator = ", ";
		[Pure] public float this[int index] => GetValues[index];

		[Pure]
		public float[] GetValues
			=> new[] {m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44};

		[Pure] public Vector3D Right => new Vector3D(m11, m12, m13);
		[Pure] public Vector3D Up => new Vector3D(m21, m22, m23);
		[Pure] public Vector3D Forward => new Vector3D(m31, m32, m33);
		[Pure] public Vector3D Backward => new Vector3D(-m31, -m32, -m33);
		[Pure] public Vector3D Translation => new Vector3D(m41, m42, m43);

		public static readonly Matrix Identity =
			new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Matrix));

		public static Matrix CreateFromScale(float scaleX, float scaleY, float scaleZ)
		{
			return new Matrix(scaleX, 0, 0, 0, 0, scaleY, 0, 0, 0, 0, scaleZ, 0, 0, 0, 0, 1);
		}

		public static Matrix CreateFromScale(Scale scale)
		{
			return new Matrix(scale.X, 0, 0, 0, 0, scale.Y, 0, 0, 0, 0, scale.Z, 0, 0, 0, 0, 1);
		}

		public static Matrix CreateRotationZYX(EulerAngles eulerAngles)
		{
			return CreateRotationZYX(eulerAngles.Pitch, eulerAngles.Yaw, eulerAngles.Roll);
		}

		// ReSharper disable once MethodTooLong
		public static Matrix CreateRotationZYX(float x, float y, float z)
		{
			var cx = MathExtensions.Cos(x);
			var sx = MathExtensions.Sin(x);
			var cy = MathExtensions.Cos(y);
			var sy = MathExtensions.Sin(y);
			var cz = MathExtensions.Cos(z);
			var sz = MathExtensions.Sin(z);
			return new Matrix(
				cy * cz, cy * sz, -sy, 0.0f,
				sx * sy * cz + cx * -sz, sx * sy * sz + cx * cz, sx * cy, 0.0f,
				cx * sy * cz + sx * sz, cx * sy * sz + -sx * cz, cx * cy, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f);
		}

		public static Matrix CreateTranslation(float x, float y, float z)
		{
			return new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, z, 1);
		}

		public static Matrix CreateTranslation(Vector3D position)
		{
			return new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, position.X, position.Y, position.Z, 1);
		}

		// ReSharper disable once MethodTooLong
		public static Matrix CreateFromScaleQuaternionAndPosition(Scale scale, Quaternion quaternion,
			Vector3D position)
		{
			var qxx = quaternion.X * quaternion.X;
			var qyy = quaternion.Y * quaternion.Y;
			var qzz = quaternion.Z * quaternion.Z;
			var qxy = quaternion.X * quaternion.Y;
			var qzw = quaternion.Z * quaternion.W;
			var qxz = quaternion.X * quaternion.Z;
			var qyw = quaternion.Y * quaternion.W;
			var qyz = quaternion.Y * quaternion.Z;
			var qxw = quaternion.X * quaternion.W;
			return new Matrix(
				scale.X * (1.0f - 2.0f * (qyy + qzz)), scale.X * 2.0f * (qxy + qzw),
				scale.X * 2.0f * (qxz - qyw), 0.0f,
				scale.Y * 2.0f * (qxy - qzw), scale.Y * (1.0f - 2.0f * (qxx + qzz)),
				scale.Y * 2.0f * (qyz + qxw), 0.0f,
				scale.Z * 2.0f * (qxz + qyw), scale.Z * 2.0f * (qyz - qxw),
				scale.Z * (1.0f - 2.0f * (qxx + qyy)), 0.0f,
				position.X, position.Y, position.Z, 1);
		}

		public static Matrix CreateOrthographicProjection(Size viewportSize)
		{
			return new Matrix(
				2.0f / viewportSize.Width, 0.0f, 0.0f, 0.0f,
				0.0f, 2.0f / -viewportSize.Height, 0.0f, 0.0f,
				0.0f, 0.0f, -1.0f, 0.0f,
				-1.0f, 1.0f, 0.0f, 1.0f);
		}

		public static Matrix CreateOrthographicProjection(Size viewportSize, float nearPlane,
			float farPlane)
		{
			var invDepth = 1.0f / (farPlane - nearPlane);
			return new Matrix(
				2.0f / viewportSize.Width, 0.0f, 0.0f, 0.0f,
				0.0f, 2.0f / viewportSize.Height, 0.0f, 0.0f,
				0.0f, 0.0f, -2.0f * invDepth, 0.0f,
				0.0f, 0.0f, -1.0f * (nearPlane + farPlane) * invDepth, 1.0f);
		}

		[Pure]
		public Matrix Transpose()
		{
			return new Matrix(
				this[0], this[4], this[8], this[12],
				this[1], this[5], this[9], this[13],
				this[2], this[6], this[10], this[14],
				this[3], this[7], this[11], this[15]);
		}

		public static Matrix CreatePerspective(float fieldOfView, float aspectRatio,
			float nearPlaneDistance, float farPlaneDistance)
		{
			var focalLength = 1.0f / MathExtensions.Tan(fieldOfView * 0.5f);
			var farPlaneByDistance = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			return new Matrix(
				focalLength / aspectRatio, 0.0f, 0.0f, 0.0f,
				0.0f, focalLength, 0.0f, 0.0f,
				0.0f, 0.0f, farPlaneByDistance, -1.0f,
				0.0f, 0.0f, nearPlaneDistance * farPlaneByDistance, 0.0f);
		}

		public static Matrix CreateCenteredQuadraticScreenSpace(float aspectRatio, Vector2D position,
			Scale zoom, Rotation2D rotation)
		{
			var width = aspectRatio < 1.0f ? 1.0f / aspectRatio : 1.0f;
			var height = aspectRatio > 1.0f ? 1.0f * aspectRatio : 1.0f;
			return CreateRotationZ(rotation.Degrees) * CreateTranslation(position) *
			       new Matrix(2.0f * width * zoom.X, 0.0f, 0.0f, 0.0f, 0.0f, 2.0f * height * zoom.Y, 0.0f,
				       0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f);
		}

		public static Matrix CreateLookAt(Vector3D cameraPosition, Vector3D cameraTarget,
			Vector3D cameraUp)
		{
			var forward = (cameraPosition - cameraTarget).Normalize();
			var side = cameraUp.Normalize().Cross(forward).Normalize();
			var up = forward.Cross(side).Normalize();
			return new Matrix(
				side.X, up.X, forward.X, 0.0f,
				side.Y, up.Y, forward.Y, 0.0f,
				side.Z, up.Z, forward.Z, 0.0f,
				-side.Dot(cameraPosition), -up.Dot(cameraPosition), -forward.Dot(cameraPosition), 1.0f);
		}

		[Pure]
		public Vector3D TransformNormal(Vector3D normal)
		{
			var values = GetValues;
			return new Vector3D(
				normal.X * values[0] + normal.Y * values[4] + normal.Z * values[8],
				normal.X * values[1] + normal.Y * values[5] + normal.Z * values[9],
				normal.X * values[2] + normal.Y * values[6] + normal.Z * values[10]);
		}

		[Pure]
		public Vector3D TransformHomogeneousCoordinate(Vector3D coord)
		{
			var values = GetValues;
			// ReSharper disable ComplexConditionExpression
			var retVector = new Vector3D(
				coord.X * values[0] + coord.Y * values[4] + coord.Z * values[8] + values[12],
				coord.X * values[1] + coord.Y * values[5] + coord.Z * values[9] + values[13],
				coord.X * values[2] + coord.Y * values[6] + coord.Z * values[10] + values[14]);
			var w = coord.X * values[3] + coord.Y * values[7] + coord.Z * values[11] + values[15];
			return retVector / w;
		}

		public static Matrix CreateRotationX(float degrees)
		{
			var cosValue = MathExtensions.Cos(degrees);
			var sinValue = MathExtensions.Sin(degrees);
			return new Matrix(
				1f, 0f, 0f, 0f,
				0f, cosValue, sinValue, 0f,
				0f, -sinValue, cosValue, 0f,
				0f, 0f, 0f, 1f);
		}

		public static Matrix CreateRotationY(float degrees)
		{
			var cosValue = MathExtensions.Cos(degrees);
			var sinValue = MathExtensions.Sin(degrees);
			return new Matrix(
				cosValue, 0f, -sinValue, 0f,
				0f, 1f, 0f, 0f,
				sinValue, 0f, cosValue, 0f,
				0f, 0f, 0f, 1f);
		}

		public static Matrix CreateRotationZ(float degrees)
		{
			var cosValue = MathExtensions.Cos(degrees);
			var sinValue = MathExtensions.Sin(degrees);
			return new Matrix(
				cosValue, sinValue, 0f, 0f,
				-sinValue, cosValue, 0f, 0f,
				0f, 0f, 1f, 0f,
				0f, 0f, 0f, 1f);
		}

		/// <summary>
		///   Further details on how to compute matrix from quaternion:
		///   http://renderfeather.googlecode.com/hg-history/034a1900d6e8b6c92440382658d2b01fc732c5de/Doc/optimized%2520Matrix%2520quaternion%2520conversion.pdf
		/// </summary>
		// ReSharper disable once MethodTooLong
		public static Matrix CreateFromQuaternion(Quaternion quaternion)
		{
			var qxx = quaternion.X * quaternion.X;
			var qyy = quaternion.Y * quaternion.Y;
			var qzz = quaternion.Z * quaternion.Z;
			var qxy = quaternion.X * quaternion.Y;
			var qzw = quaternion.Z * quaternion.W;
			var qxz = quaternion.X * quaternion.Z;
			var qyw = quaternion.Y * quaternion.W;
			var qyz = quaternion.Y * quaternion.Z;
			var qxw = quaternion.X * quaternion.W;
			return new Matrix(
				1.0f - 2.0f * (qyy + qzz), 2.0f * (qxy + qzw), 2.0f * (qxz - qyw), 0.0f,
				2.0f * (qxy - qzw), 1.0f - 2.0f * (qxx + qzz), 2.0f * (qyz + qxw), 0.0f,
				2.0f * (qxz + qyw), 2.0f * (qyz - qxw), 1.0f - 2.0f * (qxx + qyy), 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f);
		}

		public static Matrix CreateFromAxisAngle(Vector3D axis, float angle)
		{
			var quaternion = Quaternion.CreateFromAxisAngle(axis, angle);
			return CreateFromQuaternion(quaternion);
		}

		public static Matrix CreateFrustum(float left, float right, float bottom, float top, float near,
			float far)
		{
			var width = right - left;
			var height = top - bottom;
			var depth = far - near;
			var n = near * 2;
			return new Matrix(
				n / width, 0, (right + left) / width, 0,
				0, n / height, (top + bottom) / height, 0,
				0, 0, -(far + near) / depth, -(n * far) / depth,
				0, 0, -1, 1);
		}

		/// <summary>
		///   More details how to calculate Matrix Determinants: http://en.wikipedia.org/wiki/Determinant
		/// </summary>
		[Pure]
		// ReSharper disable once MethodTooLong
		public float GetDeterminant()
		{
			var det33X44 = this[15] * this[10] - this[14] * this[11];
			var det32X44 = this[9] * this[15] - this[13] * this[11];
			var det32X43 = this[9] * this[14] - this[13] * this[10];
			var det31X44 = this[8] * this[15] - this[12] * this[11];
			var det31X43 = this[8] * this[14] - this[12] * this[10];
			var det31X42 = this[8] * this[13] - this[12] * this[9];
			var det11 = this[0] * (this[5] * det33X44 - this[6] * det32X44 + this[7] * det32X43);
			var det12 = this[1] * (this[4] * det33X44 - this[6] * det31X44 + this[7] * det31X43);
			var det13 = this[2] * (this[4] * det32X44 - this[5] * det31X44 + this[7] * det31X42);
			var det14 = this[3] * (this[4] * det32X43 - this[5] * det31X43 + this[6] * det31X42);
			return det11 - det12 + det13 - det14;
		}

		[Pure]
		// ReSharper disable once MethodTooLong
		public Matrix Invert()
		{
			var values = GetValues;
			var subFactors = CalculateInvertSubFactors(values);
			// ReSharper disable once ComplexConditionExpression
			var inverse = new Matrix(
				+(values[5] * subFactors[0] - values[6] * subFactors[1] + values[7] * subFactors[2]),
				-(values[1] * subFactors[0] - values[2] * subFactors[1] + values[3] * subFactors[2]),
				+(values[1] * subFactors[6] - values[2] * subFactors[7] + values[3] * subFactors[8]),
				-(values[1] * subFactors[13] - values[2] * subFactors[14] + values[3] * subFactors[15]),
				-(values[4] * subFactors[0] - values[6] * subFactors[3] + values[7] * subFactors[4]),
				+(values[0] * subFactors[0] - values[2] * subFactors[3] + values[3] * subFactors[4]),
				-(values[0] * subFactors[6] - values[2] * subFactors[9] + values[3] * subFactors[10]),
				+(values[0] * subFactors[13] - values[2] * subFactors[16] + values[3] * subFactors[17]),
				+(values[4] * subFactors[1] - values[5] * subFactors[3] + values[7] * subFactors[5]),
				-(values[0] * subFactors[1] - values[1] * subFactors[3] + values[3] * subFactors[5]),
				+(values[0] * subFactors[11] - values[1] * subFactors[9] + values[3] * subFactors[12]),
				-(values[0] * subFactors[14] - values[1] * subFactors[16] + values[3] * subFactors[18]),
				-(values[4] * subFactors[2] - values[5] * subFactors[4] + values[6] * subFactors[5]),
				+(values[0] * subFactors[2] - values[1] * subFactors[4] + values[2] * subFactors[5]),
				-(values[0] * subFactors[8] - values[1] * subFactors[10] + values[2] * subFactors[12]),
				+(values[0] * subFactors[15] - values[1] * subFactors[17] + values[2] * subFactors[18]));
			var inverseValues = inverse.GetValues;
			// ReSharper disable once ComplexConditionExpression
			var determinant = values[0] * inverseValues[0] + values[1] * inverseValues[4] +
			                  values[2] * inverseValues[8] + values[3] * inverseValues[12];
			inverse /= determinant;
			return inverse;
		}

		// ReSharper disable once MethodTooLong
		private static float[] CalculateInvertSubFactors(float[] values)
		{
			var subFactors = new float[19];
			subFactors[0] = values[10] * values[15] - values[14] * values[11];
			subFactors[1] = values[9] * values[15] - values[13] * values[11];
			subFactors[2] = values[9] * values[14] - values[13] * values[10];
			subFactors[3] = values[8] * values[15] - values[12] * values[11];
			subFactors[4] = values[8] * values[14] - values[12] * values[10];
			subFactors[5] = values[8] * values[13] - values[12] * values[9];
			subFactors[6] = values[6] * values[15] - values[14] * values[7];
			subFactors[7] = values[5] * values[15] - values[13] * values[7];
			subFactors[8] = values[5] * values[14] - values[13] * values[6];
			subFactors[9] = values[4] * values[15] - values[12] * values[7];
			subFactors[10] = values[4] * values[14] - values[12] * values[6];
			subFactors[11] = values[5] * values[15] - values[13] * values[7];
			subFactors[12] = values[4] * values[13] - values[12] * values[5];
			subFactors[13] = values[6] * values[11] - values[10] * values[7];
			subFactors[14] = values[5] * values[11] - values[9] * values[7];
			subFactors[15] = values[5] * values[10] - values[9] * values[6];
			subFactors[16] = values[4] * values[11] - values[8] * values[7];
			subFactors[17] = values[4] * values[10] - values[8] * values[6];
			subFactors[18] = values[4] * values[9] - values[8] * values[5];
			return subFactors;
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Matrix ? Equals((Matrix) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Matrix other)
		{
			for (var i = 0; i < 16; i++)
				if (!this[i].IsNearlyEqual(other[i]))
					return false;
			return true;
		}

		[Pure]
		public static bool operator ==(Matrix matrix1, Matrix matrix2)
		{
			return matrix1.Equals(matrix2);
		}

		[Pure]
		public static bool operator !=(Matrix matrix1, Matrix matrix2)
		{
			return !matrix1.Equals(matrix2);
		}

		[Pure]
		public static Matrix operator /(Matrix matrix, float scalar)
		{
			var value = 1.0f / scalar;
			return new Matrix(
				matrix.m11 * value, matrix.m12 * value, matrix.m13 * value, matrix.m14 * value,
				matrix.m21 * value, matrix.m22 * value, matrix.m23 * value, matrix.m24 * value,
				matrix.m31 * value, matrix.m32 * value, matrix.m33 * value, matrix.m34 * value,
				matrix.m41 * value, matrix.m42 * value, matrix.m43 * value, matrix.m44 * value);
		}

		[Pure]
		public static Vector3D operator *(Matrix matrix, Vector3D vector)
		{
			return new Vector3D(
				vector.X * matrix.m11 + vector.Y * matrix.m21 + vector.Z * matrix.m31 + matrix.m41,
				vector.X * matrix.m12 + vector.Y * matrix.m22 + vector.Z * matrix.m32 + matrix.m42,
				vector.X * matrix.m13 + vector.Y * matrix.m23 + vector.Z * matrix.m33 + matrix.m43);
		}

		[Pure]
		public static Matrix operator *(Matrix matrix1, Matrix matrix2)
		{
			var result = new float[16];
			const int Dimension = 4;
			for (var i = 0; i < Dimension; i++)
			for (var j = 0; j < Dimension; j++)
			for (var k = 0; k < Dimension; k++)
				result[i * Dimension + j] += matrix1[i * Dimension + k] * matrix2[k * Dimension + j];
			return new Matrix(result);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = this[0].GetHashCode();
				for (var i = 1; i < 16; i++)
					hash = hash * 17 + this[i].GetHashCode();
				return hash;
			}
		}

		[Pure]
		public override string ToString()
		{
			return string.Join(ValueStringSeparator,
				GetValues.Select(value => value.ToInvariantString()));
		}
	}
}