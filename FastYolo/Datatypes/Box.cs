using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Min and max vector for a 3D bounding box. Sphere comparisons are faster, but many 3D
	///   models might fit better into a bounding box. Box is also easier and faster to display.
	/// </summary>
	[DebuggerDisplay("Box(min={Min}, max={Max})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Box : IEquatable<Box>
	{
		public Box(Vector3D min, Vector3D max)
		{
			Min = min;
			Max = max;
		}

		public Box(IList<Vector3D> points)
		{
			if (points == null || points.Count == 0)
				throw new NoPointsSpecified();
			Min = GetMinimumDimensions(points);
			Max = GetMaximumDimensions(points);
		}

		public class NoPointsSpecified : Exception
		{
		}

		private static Vector3D GetMinimumDimensions(IList<Vector3D> points)
		{
			return new Vector3D(FindMinimumX(points), FindMinimumY(points), FindMinimumZ(points));
		}

		private static float FindMinimumX(IList<Vector3D> points)
		{
			var x = points[0].X;
			for (var i = 1; i < points.Count; i++)
				if (points[i].X < x)
					x = points[i].X;
			return x;
		}

		private static float FindMinimumY(IList<Vector3D> points)
		{
			var y = points[0].Y;
			for (var i = 1; i < points.Count; i++)
				if (points[i].Y < y)
					y = points[i].Y;
			return y;
		}

		private static float FindMinimumZ(IList<Vector3D> points)
		{
			var z = points[0].Z;
			for (var i = 1; i < points.Count; i++)
				if (points[i].Z < z)
					z = points[i].Z;
			return z;
		}

		private static Vector3D GetMaximumDimensions(IList<Vector3D> points)
		{
			return new Vector3D(GetMaximumX(points), GetMaximumY(points), GetMaximumZ(points));
		}

		private static float GetMaximumX(IList<Vector3D> points)
		{
			var x = points[0].X;
			for (var i = 1; i < points.Count; i++)
				if (points[i].X > x)
					x = points[i].X;
			return x;
		}

		private static float GetMaximumY(IList<Vector3D> points)
		{
			var y = points[0].Y;
			for (var i = 1; i < points.Count; i++)
				if (points[i].Y > y)
					y = points[i].Y;
			return y;
		}

		private static float GetMaximumZ(IList<Vector3D> points)
		{
			var z = points[0].Z;
			for (var i = 1; i < points.Count; i++)
				if (points[i].Z > z)
					z = points[i].Z;
			return z;
		}

		[Pure] public Vector3D Min { get; }
		[Pure] public Vector3D Max { get; }

		public static Box CreateFromCenter(Vector3D position, Vector3D scale)
		{
			return new Box(position - scale / 2, position + scale / 2);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Box ? Equals((Box) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Box other)
		{
			return Min.Equals(other.Min) && Max.Equals(other.Max);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return (Min.GetHashCode() * 29) ^ Max.GetHashCode();
			}
		}

		[Pure]
		public static bool operator !=(Box box1, Box box2)
		{
			return !box1.Equals(box2);
		}

		[Pure]
		public static bool operator ==(Box box1, Box box2)
		{
			return box1.Equals(box2);
		}

		[Pure]
		public bool IsColliding(Box other)
		{
			return Max.X >= other.Min.X && Min.X <= other.Max.X && Max.Y >= other.Min.Y &&
			       Min.Y <= other.Max.Y && Max.Z >= other.Min.Z && Min.Z <= other.Max.Z;
		}

		[Pure]
		public bool IsColliding(Sphere sphere)
		{
			var clampedLocation =
				new Vector3D(sphere.Center.X > Max.X ? Max.X : Math.Max(sphere.Center.X, Min.X),
					sphere.Center.Y > Max.Y ? Max.Y : Math.Max(sphere.Center.Y, Min.Y),
					sphere.Center.Z > Max.Z ? Max.Z : Math.Max(sphere.Center.Z, Min.Z));
			return clampedLocation.DistanceToSquared(sphere.Center) <= sphere.Radius * sphere.Radius;
		}

		[Pure]
		public Vector3D? Intersect(Ray ray)
		{
			var oneOverDirection = new Vector3D(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y,
				1.0f / ray.Direction.Z);
			var minDistance = (Min - ray.Origin) * oneOverDirection;
			var maxDistance = (Max - ray.Origin) * oneOverDirection;
			return GetIntersectPointFromNearestDistance(ray, minDistance, maxDistance);
		}

		private static Vector3D? GetIntersectPointFromNearestDistance(Ray ray, Vector3D minDistance,
			Vector3D maxDistance)
		{
			var maxValue =
				Math.Min(
					Math.Min(Math.Max(minDistance.X, maxDistance.X), Math.Max(minDistance.Y, maxDistance.Y)),
					Math.Max(minDistance.Z, maxDistance.Z));
			if (maxValue < 0)
				return null;
			var minValue =
				Math.Max(
					Math.Max(Math.Min(minDistance.X, maxDistance.X), Math.Min(minDistance.Y, maxDistance.Y)),
					Math.Min(minDistance.Z, maxDistance.Z));
			if (minValue > maxValue)
				return null;
			return ray.Origin + ray.Direction * minValue;
		}

		[Pure]
		public Box Merge(Box otherBox)
		{
			return new Box(
				new Vector3D(Math.Min(Min.X, otherBox.Min.X), Math.Min(Min.Y, otherBox.Min.Y),
					Math.Min(Min.Z, otherBox.Min.Z)),
				new Vector3D(Math.Max(Max.X, otherBox.Max.X), Math.Max(Max.Y, otherBox.Max.Y),
					Math.Max(Max.Z, otherBox.Max.Z)));
		}

		/// <summary>
		///   Checks if a 3D or 2D point is inside the bounding box using IsNearlyEqual logic.
		/// </summary>
		[Pure]
		public bool Contains(Vector3D point)
		{
			return point.X + MathExtensions.Epsilon >= Min.X &&
			       point.X - MathExtensions.Epsilon <= Max.X &&
			       point.Y + MathExtensions.Epsilon >= Min.Y &&
			       point.Y - MathExtensions.Epsilon <= Max.Y &&
			       point.Z + MathExtensions.Epsilon >= Min.Z && point.Z - MathExtensions.Epsilon <= Max.Z;
		}

		[Pure]
		public override string ToString()
		{
			return "Box(Min=" + Min + ", Max=" + Max + ")";
		}
	}
}