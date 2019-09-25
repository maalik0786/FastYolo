using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Safe and lerpable rotation in degrees, stored as a float always in the interval [0, 360).
	///   Used as RenderData in Entity for 2D rotations or simple 3D rotations around the z axis, for
	///   full 3D rotations around any axis use Quaternion (as RenderData), EulerAngles or Matrix.
	///   When using float as degrees directly be careful not to interpolate anything below 0 or above
	///   360, which will cause issues. Instead use Rotation2D directly to avoid backward rotation bugs.
	/// </summary>
	[DebuggerDisplay("Rotation({" + nameof(Degrees) + "})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Rotation2D : IEquatable<Rotation2D>, LerpWithParent<Rotation2D>
	{
		public Rotation2D(float degrees)
		{
			Degrees = WrapDegreesFrom0To360(degrees);
		}

		[Pure] public float Degrees { get; }

		private static float WrapDegreesFrom0To360(float degrees)
		{
			if (degrees >= 0 && degrees < MathExtensions.FullCircleDegrees)
				return degrees;
			while (degrees < 0)
				degrees += MathExtensions.FullCircleDegrees;
			return degrees % MathExtensions.FullCircleDegrees;
		}

		public Rotation2D(string rotationAsString)
		{
			if (string.IsNullOrEmpty(rotationAsString))
				throw new InvalidNumberOfDatatypeComponents<Rotation2D>(rotationAsString);
			var values = rotationAsString.SplitIntoFloats();
			if (values.Length != 1)
				throw new InvalidNumberOfDatatypeComponents<Rotation2D>(rotationAsString);
			Degrees = WrapDegreesFrom0To360(values[0]);
		}

		public static readonly Rotation2D Zero = new Rotation2D(0);

		[Pure]
		public Rotation2D Lerp(Rotation2D other, float interpolation)
		{
			return new Rotation2D(LerpRotation(Degrees, other.Degrees, interpolation));
		}

		/// <summary>
		///   Allows to rotate and interpolate from one rotation to another without reversing direction.
		///   E.g. going from 10 degrees to 350 degrees would not work with Lerp and goes go backwards
		///   (would go all the way from 10 to 180 to 350 degrees), going from 10 to 20 degrees is fine.
		/// </summary>
		private static float LerpRotation(float degrees1, float degrees2, float percentage)
		{
			if (Math.Abs(degrees1 - degrees2) <= MathExtensions.HalfCircleDegrees)
				return degrees1.Lerp(degrees2, percentage);
			if (degrees1 > degrees2 + MathExtensions.HalfCircleDegrees)
				degrees1 -= 360;
			if (degrees1 < degrees2 - MathExtensions.HalfCircleDegrees)
				degrees1 += 360;
			return degrees1.Lerp(degrees2, percentage);
		}

		[Pure]
		public Rotation2D Lerp(Rotation2D other, float interpolation, Rotation2D addRotation)
		{
			return new Rotation2D(addRotation.Degrees +
			                      LerpRotation(Degrees, other.Degrees, interpolation));
		}

		[Pure]
		public bool Equals(Rotation2D other)
		{
			return Degrees.IsNearlyEqual(other.Degrees);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Rotation2D ? Equals((Rotation2D) other) : base.Equals(other);
		}

		[Pure]
		public static bool operator ==(Rotation2D rotation1, Rotation2D rotation2)
		{
			return rotation1.Equals(rotation2);
		}

		[Pure]
		public static bool operator !=(Rotation2D rotation1, Rotation2D rotation2)
		{
			return !rotation1.Equals(rotation2);
		}

		[Pure]
		public override int GetHashCode()
		{
			return Degrees.GetHashCode();
		}

		[Pure]
		public static Rotation2D operator *(Rotation2D currentRotation, float multiplier)
		{
			return new Rotation2D(currentRotation.Degrees * multiplier);
		}

		[Pure]
		public override string ToString()
		{
			return Degrees.ToInvariantString();
		}
	}
}