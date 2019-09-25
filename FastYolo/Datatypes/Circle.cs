using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Circle : IEquatable<Circle>, LerpWithParent<Circle>
	{
		public Circle(Vector2D center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		[Pure] public Vector2D Center { get; }

		[Pure] public float Radius { get; }

		[Pure]
		public bool Equals(Circle other)
		{
			return Center == other.Center && Radius == other.Radius;
		}

		public override bool Equals(object other)
		{
			return other is Circle ? Equals((Circle) other) : base.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Center.GetHashCode() * 397) ^ Radius.GetHashCode();
			}
		}

		public Circle Lerp(Circle other, float interpolation)
		{
			return new Circle(Center.Lerp(other.Center, interpolation),
				Radius.Lerp(other.Radius, interpolation));
		}

		public Circle Lerp(Circle other, float interpolation, Circle parentOffset)
		{
			return new Circle(Center.Lerp(other.Center, interpolation, parentOffset.Center),
				Radius.Lerp(other.Radius, interpolation) * parentOffset.Radius);
		}

		[Pure]
		public bool IsColliding(Circle other)
		{
			var combinedRadii = Radius + other.Radius;
			return Center.DistanceToSquared(other.Center) < combinedRadii * combinedRadii;
		}

		public override string ToString()
		{
			return "Circle(Center=" + Center + ", Radius=" + Radius.ToInvariantString() + ")";
		}
	}
}