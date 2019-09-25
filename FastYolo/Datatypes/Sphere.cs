using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Contains the center position in 3D and radius. Allows quick collision and intersection tests.
	/// </summary>
	[DebuggerDisplay("Sphere(Center={Center}, Radius={Radius})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Sphere : IEquatable<Sphere>
	{
		public Sphere(Vector3D center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		[Pure] public Vector3D Center { get; }
		[Pure] public float Radius { get; }

		[Pure]
		public bool IsColliding(Sphere other)
		{
			var combinedRadii = Radius + other.Radius;
			return Center.DistanceToSquared(other.Center) < combinedRadii * combinedRadii;
		}

		[Pure]
		public bool Equals(Sphere other)
		{
			return Center.Equals(other.Center) && Radius.IsNearlyEqual(other.Radius);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Sphere ? Equals((Sphere) other) : base.Equals(other);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return (Center.GetHashCode() * 13) ^ Radius.GetHashCode();
			}
		}

		[Pure]
		public static bool operator !=(Sphere sphere1, Sphere sphere2)
		{
			return !sphere1.Equals(sphere2);
		}

		[Pure]
		public static bool operator ==(Sphere sphere1, Sphere sphere2)
		{
			return sphere1.Equals(sphere2);
		}

		[Pure]
		public override string ToString()
		{
			return "Sphere(Center=" + Center + ", Radius=" + Radius.ToInvariantString() + ")";
		}
	}
}