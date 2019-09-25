using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Holds data for a rectangle by specifying its center position and the width and height.
	///   Rectangle is not LerpWithParent on purpose to prevent adding it as RenderData, use Vector2D
	///   and Size instead, so they data can be adjusted independently. For UVs use UV!
	/// </summary>
	[DebuggerDisplay("Rectangle(Center={Center}, Width={Width}, Height={Height})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Rectangle : IEquatable<Rectangle>
	{
		public Rectangle(float centerX, float centerY, float width, float height)
		{
			this.centerX = centerX;
			this.centerY = centerY;
			Width = width;
			Height = height;
		}

		private readonly float centerX;
		private readonly float centerY;

		public Rectangle(Vector2D center, Size size)
			: this(center.X, center.Y, size.Width, size.Height)
		{
		}

		public Rectangle(string rectangleAsString)
		{
			var floats = rectangleAsString.SplitIntoFloats("Center=", "Size=", ",", ";", "(", ")", " ");
			if (floats.Length != 4)
				throw new InvalidNumberOfDatatypeComponents<Rectangle>();
			centerX = floats[0];
			centerY = floats[1];
			Width = floats[2];
			Height = floats[3];
		}

		public static Rectangle CreateFromPoints(Vector2D[] points)
		{
			var left = GetLeftFromPoints(points);
			var right = GetRightFromPoints(points);
			var top = GetTopFromPoints(points);
			var bottom = GetBottomFromPoints(points);
			return new Rectangle((left + right) / 2, (top + bottom) / 2, right - left, top - bottom);
		}

		private static float GetLeftFromPoints(Vector2D[] points)
		{
			var left = float.MaxValue;
			foreach (var point in points)
				left = Math.Min(left, point.X);
			return left;
		}

		private static float GetRightFromPoints(Vector2D[] points)
		{
			var right = float.MinValue;
			foreach (var point in points)
				right = Math.Max(right, point.X);
			return right;
		}

		private static float GetTopFromPoints(Vector2D[] points)
		{
			var top = float.MinValue;
			foreach (var point in points)
				top = Math.Max(top, point.Y);
			return top;
		}

		private static float GetBottomFromPoints(Vector2D[] points)
		{
			var bottom = float.MaxValue;
			foreach (var point in points)
				bottom = Math.Min(bottom, point.Y);
			return bottom;
		}

		[Pure]
		public Rectangle Merge(Rectangle other)
		{
			var leftUpper = new Vector2D(Math.Min(Left, other.Left), Math.Min(Bottom, other.Bottom));
			var rightLower = new Vector2D(Math.Max(Right, other.Right), Math.Max(Top, other.Top));
			return CreateFromCorners(leftUpper, rightLower);
		}

		public static readonly Rectangle Zero;
		public static readonly Rectangle One = new Rectangle(0, 0, 1, 1);
		public static readonly Rectangle Half = new Rectangle(0, 0, 0.5f, 0.5f);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Rectangle));

		[Pure] public Vector2D Center => new Vector2D(centerX, centerY);
		[Pure] public float Width { get; }

		[Pure] public float Height { get; }

		[Pure] public float Left => centerX - Width / 2;
		[Pure] public float Right => centerX + Width / 2;
		[Pure] public float Bottom => centerY - Height / 2;
		[Pure] public float Top => centerY + Height / 2;
		[Pure] public Size Size => new Size(Width, Height);
		[Pure] public Vector2D BottomLeft => new Vector2D(Left, Bottom);
		[Pure] public Vector2D BottomRight => new Vector2D(Right, Bottom);
		[Pure] public Vector2D TopLeft => new Vector2D(Left, Top);
		[Pure] public Vector2D TopRight => new Vector2D(Right, Top);

		public static Rectangle CreateFromCorners(Vector2D corner1, Vector2D corner2)
		{
			return new Rectangle((corner2.X + corner1.X) / 2, (corner2.Y + corner1.Y) / 2,
				Math.Abs(corner2.X - corner1.X), Math.Abs(corner2.Y - corner1.Y));
		}

		[Pure]
		public bool Contains(Vector2D point)
		{
			return point.X >= Left && point.X <= Right && point.Y >= Bottom && point.Y <= Top;
		}

		[Pure]
		public bool Intersects(Rectangle inner)
		{
			return Contains(inner.TopLeft) || Contains(inner.TopRight) || Contains(inner.BottomLeft) ||
			       Contains(inner.BottomRight);
		}

		[Pure] public float AspectRatio => Width / Height;

		[Pure]
		public Rectangle Increase(Size size)
		{
			return new Rectangle(centerX, centerY, Width + size.Width, Height + size.Height);
		}

		[Pure]
		public Rectangle Reduce(Size size)
		{
			return new Rectangle(centerX, centerY, Width - size.Width, Height - size.Height);
		}

		[Pure]
		public Rectangle Combine(Rectangle other)
		{
			return CreateFromCorners(
				new Vector2D(Math.Min(Left, other.Left), Math.Min(Bottom, other.Bottom)),
				new Vector2D(Math.Max(Right, other.Right), Math.Max(Top, other.Top)));
		}

		[Pure]
		public Rectangle GetInnerRectangle(Rectangle relativeRectangle)
		{
			return new Rectangle(centerX + Width * relativeRectangle.centerX,
				centerY + Height * relativeRectangle.centerY, Width * relativeRectangle.Width,
				Height * relativeRectangle.Height);
		}

		[Pure]
		public Vector2D GetRelativePoint(Vector2D pointRelativeToCenter)
		{
			return new Vector2D((pointRelativeToCenter.X - centerX) / Width,
				(pointRelativeToCenter.Y - centerY) / Height);
		}

		[Pure]
		public Vector2D GetRandomPoint(Randomizer random)
		{
			return new Vector2D(Left + random.Get(0, Width), Bottom + random.Get(0, Height));
		}

		[Pure]
		public Rectangle Move(Vector2D translation)
		{
			return new Rectangle(centerX + translation.X, centerY + translation.Y, Width, Height);
		}

		[Pure]
		public Rectangle Move(float translationX, float translationY)
		{
			return new Rectangle(centerX + translationX, centerY + translationY, Width, Height);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Rectangle ? Equals((Rectangle) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Rectangle other)
		{
			return centerX.IsNearlyEqual(other.centerX) && centerY.IsNearlyEqual(other.centerY) &&
			       Width.IsNearlyEqual(other.Width) && Height.IsNearlyEqual(other.Height);
		}

		[Pure]
		public static Rectangle operator *(Rectangle rect, float scale)
		{
			return new Rectangle(rect.Center, rect.Size * scale);
		}

		[Pure]
		public static bool operator ==(Rectangle rect1, Rectangle rect2)
		{
			return rect1.Equals(rect2);
		}

		[Pure]
		public static bool operator !=(Rectangle rect1, Rectangle rect2)
		{
			return !rect1.Equals(rect2);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return centerX.GetHashCode() * 511 + centerY.GetHashCode() * 111 +
				       Width.GetHashCode() * 17 + Height.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return "Center=" + centerX.ToInvariantString() + ", " + centerY.ToInvariantString() +
			       ", Size=" +
			       Width.ToInvariantString() + ", " + Height.ToInvariantString();
		}

		[Pure]
		public bool IsColliding(Rectangle otherRect)
		{
			return otherRect.Right >= centerX - Width / 2 && otherRect.Left < centerX + Width / 2 &&
			       otherRect.Top >= centerY - Height / 2 && otherRect.Bottom < centerY + Height / 2;
		}

		[Pure]
		public bool IsColliding(float rotation, Rectangle otherRect, float otherRotation)
		{
			if (rotation == 0 && otherRotation == 0)
				return IsColliding(otherRect);
			var rotatedRect = GetRotatedRectangleCorners(Center, rotation);
			var rotatedOtherRect = otherRect.GetRotatedRectangleCorners(otherRect.Center, otherRotation);
			foreach (var axis in GetAxes(rotatedRect, rotatedOtherRect))
				if (IsProjectedAxisOutsideRectangles(axis, rotatedRect, rotatedOtherRect))
					return false;
			return true;
		}

		[Pure]
		public Vector2D[] GetRotatedRectangleCorners(Vector2D center, float rotation)
		{
			return new[]
			{
				TopLeft.RotateAround(center, rotation), BottomLeft.RotateAround(center, rotation),
				BottomRight.RotateAround(center, rotation), TopRight.RotateAround(center, rotation)
			};
		}

		private static Vector2D[] GetAxes(Vector2D[] rectangle, Vector2D[] otherRect)
		{
			return new[]
			{
				new Vector2D(rectangle[1].X - rectangle[0].X, rectangle[1].Y - rectangle[0].Y),
				new Vector2D(rectangle[1].X - rectangle[2].X, rectangle[1].Y - rectangle[2].Y),
				new Vector2D(otherRect[0].X - otherRect[3].X, otherRect[0].Y - otherRect[3].Y),
				new Vector2D(otherRect[0].X - otherRect[1].X, otherRect[0].Y - otherRect[1].Y)
			};
		}

		private static bool IsProjectedAxisOutsideRectangles(Vector2D axis, Vector2D[] rotatedRect,
			Vector2D[] rotatedOtherRect)
		{
			var rectMin = float.MaxValue;
			var rectMax = float.MinValue;
			var otherMin = float.MaxValue;
			var otherMax = float.MinValue;
			GetRectangleProjectionResult(axis, rotatedRect, ref rectMin, ref rectMax);
			GetRectangleProjectionResult(axis, rotatedOtherRect, ref otherMin, ref otherMax);
			return rectMin > otherMax || rectMax < otherMin;
		}

		private static void GetRectangleProjectionResult(Vector2D axis, Vector2D[] cornerList,
			ref float min, ref float max)
		{
			foreach (var corner in cornerList)
			{
				var projectedValueX = corner.DistanceFromProjectAxisPointX(axis) * axis.X;
				var projectedValueY = corner.DistanceFromProjectAxisPointY(axis) * axis.Y;
				var projectedValue = projectedValueX + projectedValueY;
				if (projectedValue < min)
					min = projectedValue;
				if (projectedValue > max)
					max = projectedValue;
			}
		}

		[Pure]
		public bool Intersects(Circle circle)
		{
			return new Vector2D(GetClampedX(circle), GetClampedY(circle))
				       .DistanceToSquared(circle.Center) <=
			       circle.Radius * circle.Radius;
		}

		private float GetClampedX(Circle circle)
		{
			return circle.Center.X > Right ? Right : circle.Center.X < Left ? Left : circle.Center.X;
		}

		private float GetClampedY(Circle circle)
		{
			return circle.Center.Y > Top ? Top : circle.Center.Y < Bottom ? Bottom : circle.Center.Y;
		}

		[Pure]
		public Rectangle GetBoundingBoxAfterRotation(float angle)
		{
			return CreateFromPoints(GetRotatedRectangleCorners(Center, angle));
		}

		/// <summary>
		///   When an <see cref="Intersects(FastYolo.Datatypes.Circle)" /> collision with this rectangle
		///   happens it is already too late and the collision point is already inside the rectangle. This
		///   method gives us the circle position at the actual border of the rectangle collision.
		/// </summary>
		[Pure]
		public Vector2D GetOutsideCollisionPoint(Vector2D previousPosition, Circle collidedCircle)
		{
			// Go through each of the 4 edges of the rectangle and if there is a collision, return
			var line = new Line2DData(previousPosition, collidedCircle.Center);
			// Bottom edge intersection?
			var intersection =
				line.GetLineSegmentIntersectionPoint(new Line2DData(BottomLeft, BottomRight));
			if (intersection != null)
				return intersection.Value + new Vector2D(0, -collidedCircle.Radius);
			// Right edge?
			intersection = line.GetLineSegmentIntersectionPoint(new Line2DData(BottomRight, TopRight));
			if (intersection != null)
				return intersection.Value + new Vector2D(collidedCircle.Radius, 0);
			// Top edge?
			intersection = line.GetLineSegmentIntersectionPoint(new Line2DData(TopRight, TopLeft));
			if (intersection != null)
				return intersection.Value + new Vector2D(0, collidedCircle.Radius);
			// Left edge?
			intersection = line.GetLineSegmentIntersectionPoint(new Line2DData(TopLeft, BottomLeft));
			if (intersection != null)
				return intersection.Value + new Vector2D(-collidedCircle.Radius, 0);
			return previousPosition; //ncrunch: no coverage
		}
	}
}