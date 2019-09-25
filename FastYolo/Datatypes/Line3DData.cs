using System;
using System.Diagnostics.Contracts;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	public struct Line3DData : LerpWithParent<Line3DData>, IEquatable<Line3DData>
	{
		public Line3DData(Vector3D start, Vector3D end)
			: this()
		{
			Start = start;
			End = end;
		}

		public Line3DData(string lineAsString)
		{
			var parts = lineAsString.SplitAndTrim("Start:", "End:", ";");
			if (parts.Length != 2)
				throw new InvalidNumberOfDatatypeComponents<Line3DData>(lineAsString);
			Start = new Vector3D(parts[0]);
			End = new Vector3D(parts[1]);
		}

		[Pure] public Vector3D Start { get; }

		[Pure] public Vector3D End { get; }

		[Pure] public float Length => Start.DistanceTo(End);

		public override bool Equals(object other)
		{
			return other is Line3DData ? Equals((Line3DData) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Line3DData other)
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
		public Line3DData Lerp(Line3DData other, float interpolation, Line3DData parentOffset)
		{
			return new Line3DData(Start.Lerp(other.Start, interpolation) + parentOffset.Start,
				End.Lerp(other.End, interpolation) + parentOffset.End);
		}

		[Pure]
		public Line3DData Lerp(Line3DData other, float interpolation)
		{
			return new Line3DData(Start.Lerp(other.Start, interpolation),
				End.Lerp(other.End, interpolation));
		}

		[Pure]
		public override string ToString()
		{
			return "Start: " + Start + ", End: " + End;
		}
	}
}