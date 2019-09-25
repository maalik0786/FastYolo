using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Plane struct represented by a normal vector and a distance from the origin.
	///   Details can be found at: http://en.wikipedia.org/wiki/Plane_%28geometry%29
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Plane : IEquatable<Plane>
	{
		public Plane(Vector3D normal, float distance)
		{
			Normal = normal.Normalize();
			Distance = distance;
		}

		public Plane(Vector3D normal, Vector3D vectorOnPlane)
		{
			Normal = normal.Normalize();
			Distance = -normal.Dot(vectorOnPlane);
		}

		[Pure] public Vector3D Normal { get; }

		[Pure] public float Distance { get; }

		[Pure]
		public Vector3D? Intersect(Ray ray)
		{
			// ReSharper disable ImpureMethodCallOnReadonlyValueField
			var numerator = Normal.Dot(ray.Origin) + Distance;
			var denominator = Normal.Dot(ray.Direction);
			if (denominator.IsNearlyEqual(0.0f))
				return null;
			var intersectionDistance = -(numerator / denominator);
			if (intersectionDistance < 0.0f)
				return null;
			return ray.Origin + ray.Direction * intersectionDistance;
		}

		public override bool Equals(object other)
		{
			return other is Plane ? Equals((Plane) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Plane other)
		{
			return Normal.Equals(other.Normal) && Distance.IsNearlyEqual(other.Distance);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Normal.GetHashCode() * 397) ^ Distance.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return "Normal: " + Normal + ", Distance: " + Distance;
		}
	}
}