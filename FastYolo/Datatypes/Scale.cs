using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Same as Vector3D with less functionality, but is a different type to easily support scaling
	///   in Entities (2D or 3D), normally uniform. Scale is done on top of the Size render data in 2D
	///   or on top of Box for meshes in 3D. Children entities will scale with their parents.
	/// </summary>
	[DebuggerDisplay("Scale({X}, {Y}, {Z})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Scale : IEquatable<Scale>, LerpWithParent<Scale>
	{
		public Scale(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		[Pure] public float X { get; }
		[Pure] public float Y { get; }
		[Pure] public float Z { get; }

		public Scale(float scaleFactorForXyz)
		{
			X = scaleFactorForXyz;
			Y = scaleFactorForXyz;
			Z = scaleFactorForXyz;
		}

		public Scale(float x, float y)
		{
			X = x;
			Y = y;
			Z = x;
		}

		public Scale(Size scaleFactorFromSize)
		{
			X = scaleFactorFromSize.Width;
			Y = scaleFactorFromSize.Height;
			Z = X;
		}

		public Scale(string scaleXyzAsString)
		{
			var components = scaleXyzAsString.SplitIntoFloats();
			if (components.Length < 1 || components.Length > 3)
				throw new InvalidNumberOfDatatypeComponents<Scale>();
			X = components[0];
			Y = components.Length > 1 ? components[1] : X;
			Z = components.Length > 2 ? components[2] : X;
		}

		public static readonly Scale Zero;
		public static readonly Scale Half = new Scale(0.5f, 0.5f, 0.5f);
		public static readonly Scale One = new Scale(1, 1, 1);

		[Pure]
		public bool Equals(Scale other)
		{
			return X.IsNearlyEqual(other.X) && Y.IsNearlyEqual(other.Y) && Z.IsNearlyEqual(other.Z);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Scale ? Equals((Scale) other) : base.Equals(other);
		}

		[Pure]
		public static bool operator !=(Scale size1, Scale size2)
		{
			return !size1.Equals(size2);
		}

		[Pure]
		public static bool operator ==(Scale size1, Scale size2)
		{
			return size1.Equals(size2);
		}

		[Pure]
		public static Scale operator *(Scale s, float f)
		{
			return new Scale(s.X * f, s.Y * f, s.Z * f);
		}

		[Pure]
		public static Scale operator *(float f, Scale s)
		{
			return new Scale(f * s.X, f * s.Y, f * s.Z);
		}

		[Pure]
		public static Scale operator *(Scale left, Scale right)
		{
			return new Scale(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		[Pure]
		public static Scale operator /(Scale s, float f)
		{
			return new Scale(s.X / f, s.Y / f, s.Z / f);
		}

		[Pure]
		public static Scale operator /(float f, Scale s)
		{
			return new Scale(f / s.X, f / s.Y, f / s.Z);
		}

		[Pure]
		public static Scale operator /(Scale s1, Scale s2)
		{
			return new Scale(s1.X / s2.X, s1.Y / s2.Y, s1.Z / s2.Z);
		}

		[Pure]
		public static Scale operator +(Scale left, Scale right)
		{
			return new Scale(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return X.GetHashCode() * 134 + Y.GetHashCode() * 523 + Z.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return X.ToInvariantString() + ", " + Y.ToInvariantString() + ", " + Z.ToInvariantString();
		}

		[Pure]
		public Scale Lerp(Scale other, float interpolation)
		{
			return new Scale(X.Lerp(other.X, interpolation), Y.Lerp(other.Y, interpolation),
				Z.Lerp(other.Z, interpolation));
		}

		[Pure]
		public Scale Lerp(Scale other, float interpolation, Scale parentScale)
		{
			return parentScale * Lerp(other, interpolation);
		}
	}
}