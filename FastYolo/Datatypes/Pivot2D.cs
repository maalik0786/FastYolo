using System;
using System.Diagnostics.Contracts;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   By default all 2D entities are centered at their position and all operations (scaling,
	///   rotating, children centering) are done at that position. Use this Pivot2D component to change
	///   the center to something else, all operations will be centered at that relative point instead.
	///   Children positioning is directly affected too, but children do not inherit the pivot point.
	/// </summary>
	public struct Pivot2D : LerpWithParent<Pivot2D>, IEquatable<Pivot2D>
	{
		public Pivot2D(Vector2D point)
		{
			Point = point;
		}

		public Pivot2D(string pointAsString)
		{
			Point = new Vector2D(pointAsString);
		}

		[Pure] public Vector2D Point { get; }
		public static readonly Pivot2D Zero = new Pivot2D(Vector2D.Zero);

		[Pure]
		public Pivot2D Lerp(Pivot2D other, float interpolation)
		{
			return new Pivot2D(Point.Lerp(other.Point, interpolation));
		}

		/// <summary>
		///   Not used for lerp with parent. Used instead in IncreaseRenderDataPropertyMessage for
		///   increasing Pivot2D values. See RenderDataArray and SpriteRenderer for ignoring the parent.
		/// </summary>
		[Pure]
		public Pivot2D Lerp(Pivot2D other, float interpolation, Pivot2D parentOffset)
		{
			return new Pivot2D(Point.Lerp(other.Point, interpolation, parentOffset.Point));
		}

		public override bool Equals(object other)
		{
			return other is Pivot2D ? Equals((Pivot2D) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Pivot2D other)
		{
			return Point.Equals(other.Point);
		}

		[Pure]
		public override int GetHashCode()
		{
			return Point.GetHashCode();
		}

		[Pure]
		public override string ToString()
		{
			return Point.ToString();
		}
	}
}