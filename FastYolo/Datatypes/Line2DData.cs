using System;
using System.Diagnostics.Contracts;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	[Serializable]
	public struct Line2DData : LerpWithParent<Line2DData>, IEquatable<Line2DData>
	{
		public Line2DData(Vector2D start, Vector2D end) : this()
		{
			Start = start;
			End = end;
		}

		public Line2DData(string lineAsString)
		{
			var parts = lineAsString.SplitAndTrim("Start:", "End:", ";");
			if (parts.Length != 2)
				throw new InvalidNumberOfDatatypeComponents<Line2DData>(lineAsString);
			Start = new Vector2D(parts[0]);
			End = new Vector2D(parts[1]);
		}

		[Pure] public Vector2D Start { get; }

		[Pure] public Vector2D End { get; }

		[Pure] public float Length => Start.DistanceTo(End);

		public override bool Equals(object other)
		{
			return other is Line2DData ? Equals((Line2DData) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Line2DData other)
		{
			return Start.Equals(other.Start) && End.Equals(other.End);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Start.GetHashCode() * 397) ^ End.GetHashCode();
			}
		}

		[Pure]
		public Line2DData Lerp(Line2DData other, float interpolation, Line2DData parentOffset)
		{
			return new Line2DData(Start.Lerp(other.Start, interpolation) + parentOffset.Start,
				End.Lerp(other.End, interpolation) + parentOffset.End);
		}

		[Pure]
		public Line2DData Lerp(Line2DData other, float interpolation)
		{
			return new Line2DData(Start.Lerp(other.Start, interpolation),
				End.Lerp(other.End, interpolation));
		}

		[Pure]
		public override string ToString()
		{
			return "Start: " + Start + "; End: " + End;
		}

		/// <summary>
		///   https://gamedev.stackexchange.com/questions/111100/intersection-of-a-line-and-a-rectangle
		///   Will return null if the lines do not touch each other, making this useful for rectangles.
		/// </summary>
		[Pure]
		public Vector2D? GetLineSegmentIntersectionPoint(Line2DData otherLine)
		{
			var ourSegment = End - Start;
			var otherSegment = otherLine.End - otherLine.Start;
			var delta = ourSegment.Y * otherSegment.X - otherSegment.Y * ourSegment.X;
			if (delta == 0)
				return null;
			var c1 = ourSegment.Y * Start.X + ourSegment.X * End.Y;
			var c2 = otherSegment.Y * otherLine.Start.X + otherSegment.X * otherLine.Start.Y;
			var invertedDelta = 1.0f / delta;
			var result = new Vector2D((otherSegment.X * c1 - ourSegment.X * c2) * invertedDelta,
				(ourSegment.Y * c2 - otherSegment.Y * c1) * invertedDelta);
			if (result.IsBetweenTwoPoints(Start, End))
				return result;
			return null;
		}
	}
}