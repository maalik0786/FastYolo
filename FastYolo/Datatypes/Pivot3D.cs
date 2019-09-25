using System;
using System.Diagnostics.Contracts;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   By default all 3D entities are centered at their position and all operations (scaling,
	///   orientation, matrix and children centering) are done at that position. Use this Pivot3D
	///   component to change the center, operations will be centered at that relative point instead.
	/// </summary>
	public struct Pivot3D : LerpWithParent<Pivot3D>, IEquatable<Pivot3D>
	{
		public Pivot3D(Vector3D position)
		{
			Position = position;
		}

		public Pivot3D(string positionAsString)
		{
			Position = new Vector3D(positionAsString);
		}

		[Pure] public Vector3D Position { get; }
		public static readonly Pivot3D Zero = new Pivot3D(Vector3D.Zero);

		[Pure]
		public Pivot3D Lerp(Pivot3D other, float interpolation)
		{
			return new Pivot3D(Position.Lerp(other.Position, interpolation));
		}

		[Pure]
		public Pivot3D Lerp(Pivot3D other, float interpolation, Pivot3D parentOffset)
		{
			return new Pivot3D(Position.Lerp(other.Position, interpolation, parentOffset.Position));
		}

		public override bool Equals(object other)
		{
			return other is Pivot3D ? Equals((Pivot3D) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Pivot3D other)
		{
			return Position.Equals(other.Position);
		}

		[Pure]
		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}

		[Pure]
		public override string ToString()
		{
			return Position.ToString();
		}
	}
}