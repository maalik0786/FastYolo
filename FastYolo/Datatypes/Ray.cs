using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Ray struct, used to fire rays into a 3D scene to find out what we can
	///   hit with that ray (for mouse picking and other simple 3D collision stuff).
	/// </summary>
	[DebuggerDisplay("Ray(Origin {Origin}, Direction {Direction})")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Ray : IEquatable<Ray>
	{
		public Ray(Vector3D origin, Vector3D direction)
		{
			Origin = origin;
			Direction = direction;
		}

		[Pure] public Vector3D Origin { get; }
		[Pure] public Vector3D Direction { get; }

		public Ray(string stringRay)
		{
			var values = stringRay.SplitIntoFloats();
			if (values.Length != 6)
				throw new InvalidNumberOfDatatypeComponents<Ray>();
			Origin = new Vector3D(values[0], values[1], values[2]);
			Direction = new Vector3D(values[3], values[4], values[5]);
		}

		public override bool Equals(object other)
		{
			return other is Ray ? Equals((Ray) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Ray other)
		{
			return Origin == other.Origin && Direction == other.Direction;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Origin.GetHashCode() * 397) ^ Direction.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return Origin + "; " + Direction;
		}
	}
}