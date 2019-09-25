using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Represents a 2D vector, which is useful for screen positions (sprites, mouse, touch, etc.)
	/// </summary>
	[DebuggerDisplay("Vector2D({X}, {Y})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Vector2D : IEquatable<Vector2D>, IComparable<Vector2D>, LerpWithParent<Vector2D>
	{
		public Vector2D(float x, float y)
		{
			X = x;
			Y = y;
		}

		public Vector2D(string vectorAsString)
		{
			var components = vectorAsString.SplitIntoFloats();
			if (components.Length != 2)
				throw new InvalidNumberOfDatatypeComponents<Vector2D>();
			X = components[0];
			Y = components[1];
		}

		public static readonly Vector2D Zero;
		public static readonly Vector2D One = new Vector2D(1, 1);
		public static readonly Vector2D Half = new Vector2D(0.5f, 0.5f);
		public static readonly Vector2D Left = new Vector2D(-1, 0);
		public static readonly Vector2D Right = new Vector2D(1, 0);
		public static readonly Vector2D Up = new Vector2D(0, 1);
		public static readonly Vector2D Down = new Vector2D(0, -1);
		public static readonly Vector2D Unused = new Vector2D(-1, -1);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2D));
		[Pure] public float X { get; }

		[Pure] public float Y { get; }

		public static Vector2D FromAngle(float rotationAngleInDegrees)
		{
			return new Vector2D(1, 0).Rotate(rotationAngleInDegrees);
		}

		[Pure]
		public static Vector2D operator +(Vector2D vector1, Vector2D vector2)
		{
			return new Vector2D(vector1.X + vector2.X, vector1.Y + vector2.Y);
		}

		[Pure]
		public static Vector2D operator -(Vector2D vector1, Vector2D vector2)
		{
			return new Vector2D(vector1.X - vector2.X, vector1.Y - vector2.Y);
		}

		[Pure]
		public static Vector2D operator *(float f, Vector2D vector)
		{
			return new Vector2D(vector.X * f, vector.Y * f);
		}

		[Pure]
		public static Vector2D operator *(Vector2D vector, float f)
		{
			return f * vector;
		}

		[Pure]
		public static Vector2D operator *(Vector2D vector, Scale scale)
		{
			return new Vector2D(vector.X * scale.X, vector.Y * scale.Y);
		}

		[Pure]
		public static Vector2D operator /(Vector2D vector, float f)
		{
			return new Vector2D(vector.X / f, vector.Y / f);
		}

		[Pure]
		public static Vector2D operator /(Vector2D vector, Scale scale)
		{
			return new Vector2D(vector.X / scale.X, vector.Y / scale.Y);
		}

		[Pure]
		public static Vector2D operator -(Vector2D vector)
		{
			return new Vector2D(-vector.X, -vector.Y);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Vector2D ? Equals((Vector2D) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Vector2D other)
		{
			return X.IsNearlyEqual(other.X) && Y.IsNearlyEqual(other.Y);
		}

		[Pure]
		public static bool operator ==(Vector2D vector1, Vector2D vector2)
		{
			return vector1.Equals(vector2);
		}

		[Pure]
		public static bool operator !=(Vector2D vector1, Vector2D vector2)
		{
			return !vector1.Equals(vector2);
		}

		[Pure]
		public static implicit operator Vector2D(Size size)
		{
			return new Vector2D(size.Width, size.Height);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return X.GetHashCode() * 23 + Y.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return X.ToInvariantString() + ", " + Y.ToInvariantString();
		}

		[Pure] public float Length => MathExtensions.Sqrt(LengthSquared);
		[Pure] public float LengthSquared => X * X + Y * Y;

		[Pure]
		public float GetRotation()
		{
			if (X == 0 && Y == 0)
				return 0;
			var normalized = Normalize();
			return MathExtensions.Atan2(normalized.Y, normalized.X);
		}

		[Pure]
		public float DistanceTo(Vector2D other)
		{
			return MathExtensions.Sqrt(DistanceToSquared(other));
		}

		[Pure]
		public float DistanceToSquared(Vector2D other)
		{
			var distanceX = X - other.X;
			var distanceY = Y - other.Y;
			return distanceX * distanceX + distanceY * distanceY;
		}

		[Pure]
		public Vector2D DirectionTo(Vector2D other)
		{
			return other - this;
		}

		[Pure]
		public Vector2D ReflectIfHittingBorder(Rectangle box, Rectangle borders)
		{
			var bouncedX = X;
			var bouncedY = Y;
			if (box.Left <= borders.Left)
				bouncedX = X.Abs();
			if (box.Right >= borders.Right)
				bouncedX = -X.Abs();
			if (box.Bottom <= borders.Bottom)
				bouncedY = Y.Abs();
			if (box.Top >= borders.Top)
				bouncedY = -Y.Abs();
			return new Vector2D(bouncedX, bouncedY);
		}

		[Pure]
		public Vector2D RestrictToArea(Size size, Rectangle borders)
		{
			var allowedArea = borders.Reduce(size);
			return new Vector2D(
				X < allowedArea.Left ? allowedArea.Left : X > allowedArea.Right ? allowedArea.Right : X,
				Y < allowedArea.Bottom ? allowedArea.Bottom : Y > allowedArea.Top ? allowedArea.Top : Y);
		}

		/// <summary>
		///   This is the most used Lerp method, which is why we inlined the MathExtensions.Lerp call.
		/// </summary>
		public Vector2D Lerp(Vector2D other, float interpolation)
		{
			return new Vector2D(X * (1 - interpolation) + other.X * interpolation,
				Y * (1 - interpolation) + other.Y * interpolation);
		}

		public Vector2D Lerp(Vector2D other, float interpolation, Vector2D parentVector)
		{
			return new Vector2D(parentVector.X + X * (1 - interpolation) + other.X * interpolation,
				parentVector.Y + Y * (1 - interpolation) + other.Y * interpolation);
		}

		[Pure]
		public Vector2D Rotate(float angleInDegrees)
		{
			if (angleInDegrees.IsNearlyEqual(0))
				return this;
			var rotationSin = MathExtensions.Sin(angleInDegrees);
			var rotationCos = MathExtensions.Cos(angleInDegrees);
			return new Vector2D(X * rotationCos - Y * rotationSin, X * rotationSin + Y * rotationCos);
		}

		[Pure]
		public Vector2D Rotate(float rotationSin, float rotationCos)
		{
			return new Vector2D(X * rotationCos - Y * rotationSin, X * rotationSin + Y * rotationCos);
		}

		[Pure]
		public Vector2D RotateAround(Vector2D pivot, float angleInDegrees)
		{
			if (angleInDegrees.IsNearlyEqual(0))
				return this;
			return RotateAround(pivot, MathExtensions.Sin(angleInDegrees),
				MathExtensions.Cos(angleInDegrees));
		}

		[Pure]
		public Vector2D RotateAround(Vector2D pivot, float rotationSin, float rotationCos)
		{
			var translatedPointX = X - pivot.X;
			var translatedPointY = Y - pivot.Y;
			return new Vector2D(
				pivot.X + translatedPointX * rotationCos - translatedPointY * rotationSin,
				pivot.Y + translatedPointX * rotationSin + translatedPointY * rotationCos);
		}

		[Pure]
		public Vector2D RotateAndScaleAround(Vector2D pivot, float rotationSin, float rotationCos,
			Scale scale)
		{
			var translatedPointX = X - pivot.X;
			var translatedPointY = Y - pivot.Y;
			return new Vector2D(
				pivot.X + translatedPointX * rotationCos * scale.X -
				translatedPointY * rotationSin * scale.Y,
				pivot.Y + translatedPointX * rotationSin * scale.X +
				translatedPointY * rotationCos * scale.Y);
		}

		[Pure]
		public float RotationTo(Vector2D target)
		{
			var normal = (this - target).Normalize();
			return MathExtensions.Atan2(normal.Y, normal.X);
		}

		[Pure]
		public Vector2D Normalize()
		{
			return new Vector2D(X / Length, Y / Length);
		}

		[Pure]
		public float DotProduct(Vector2D vector)
		{
			return X * vector.X + Y * vector.Y;
		}

		[Pure]
		public float DistanceFromProjectAxisPointX(Vector2D axis)
		{
			return (X * axis.X + Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y) * axis.X;
		}

		[Pure]
		public float DistanceFromProjectAxisPointY(Vector2D axis)
		{
			return (X * axis.X + Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y) * axis.Y;
		}

		/// <summary>
		///   http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
		/// </summary>
		[Pure]
		// ReSharper disable once MethodTooLong
		public float DistanceToLine(Vector2D lineStart, Vector2D lineEnd)
		{
			var lineDirection = lineEnd - lineStart;
			var lineLengthSquared = lineDirection.LengthSquared;
			if (lineLengthSquared == 0.0)
				return DistanceTo(lineStart);
			var startDirection = this - lineStart;
			var linePosition = startDirection.DotProduct(lineDirection) / lineLengthSquared;
			var projection = lineStart + linePosition * lineDirection;
			return DistanceTo(projection);
		}

		[Pure]
		// ReSharper disable once MethodTooLong
		public float DistanceToLineSquared(Vector2D lineStart, Vector2D lineEnd)
		{
			var lineDirection = lineEnd - lineStart;
			var lineLengthSquared = lineDirection.LengthSquared;
			if (lineLengthSquared == 0.0)
				return DistanceToSquared(lineStart);
			var startDirection = this - lineStart;
			var linePosition = startDirection.DotProduct(lineDirection) / lineLengthSquared;
			var projection = lineStart + linePosition * lineDirection;
			return DistanceToSquared(projection);
		}

		/// <summary>
		///   http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
		/// </summary>
		[Pure]
		// ReSharper disable once MethodTooLong
		public float DistanceToLineSegment(Vector2D lineStart, Vector2D lineEnd)
		{
			var lineDirection = lineEnd - lineStart;
			var lineLengthSquared = lineDirection.LengthSquared;
			if (lineLengthSquared == 0.0)
				return DistanceTo(lineStart);
			var startDirection = this - lineStart;
			var linePosition = startDirection.DotProduct(lineDirection) / lineLengthSquared;
			if (linePosition < 0.0)
				return DistanceTo(lineStart);
			if (linePosition > 1.0)
				return DistanceTo(lineEnd);
			var projection = lineStart + linePosition * lineDirection;
			return DistanceTo(projection);
		}

		/// <summary>
		///   http://stackoverflow.com/questions/3461453/determine-which-side-of-a-line-a-point-lies
		/// </summary>
		[Pure]
		public bool IsLeftOfLineOrOnIt(Vector2D lineStart, Vector2D lineEnd)
		{
			return (lineEnd.X - lineStart.X) * (Y - lineStart.Y) -
			       (lineEnd.Y - lineStart.Y) * (X - lineStart.X) >= 0;
		}

		[Pure]
		public float Angle(Vector2D other)
		{
			var dot = DotProduct(other);
			var cosine = dot / (Length * other.Length);
			if (cosine >= 1.0f)
				return 0;
			if (cosine <= -1.0f)
				return MathExtensions.HalfCircleDegrees;
			return MathExtensions.Acos(cosine);
		}

		/// <summary>
		///   http://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
		/// </summary>
		[Pure]
		public bool IsLineIntersectingWith(Vector2D line1End, Vector2D line2Start, Vector2D line2End)
		{
			// ReSharper disable ComplexConditionExpression
			var denominator = (line2End.Y - line2Start.Y) * (line1End.X - X) -
			                  (line2End.X - line2Start.X) * (line1End.Y - Y);
			var ua = ((line2End.X - line2Start.X) * (Y - line2Start.Y) -
			          (line2End.Y - line2Start.Y) * (X - line2Start.X)) / denominator;
			var ub = ((line1End.X - X) * (Y - line2Start.Y) -
			          (line1End.Y - Y) * (X - line2Start.X)) / denominator;
			return ua >= 0f && ua <= 1f && ub >= 0f && ub <= 1f;
		}

		[Pure]
		public Vector2D LimitTo(Rectangle maxArea)
		{
			return new Vector2D(X < maxArea.Left ? maxArea.Left : X > maxArea.Right ? maxArea.Right : X,
				Y < maxArea.Bottom ? maxArea.Bottom : Y > maxArea.Top ? maxArea.Top : Y);
		}

		public int CompareTo(Vector2D other)
		{
			var xComparison = X.CompareTo(other.X);
			return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
		}

		[Pure]
		public bool IsBetweenTwoPoints(Vector2D start, Vector2D end)
		{
			return Math.Min(start.X, end.X).IsLessOrNearlyEqual(X) &&
			       X.IsLessOrNearlyEqual(Math.Max(start.X, end.X)) &&
			       Math.Min(start.Y, end.Y).IsLessOrNearlyEqual(Y) &&
			       Y.IsLessOrNearlyEqual(Math.Max(start.Y, end.Y));
		}
	}
}