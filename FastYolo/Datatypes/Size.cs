using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Holds the width and height of an object (e.g. a rectangle or the screen resolution). This does
	///   not implements LerpWithParent on purpose, use Scale for scaling RenderData and affecting all
	///   children. SizeComponent holds the size for 2D entities and detects changes for optimizations.
	/// </summary>
	[DebuggerDisplay("Size({Width}, {Height})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Size : IEquatable<Size>
	{
		public Size(float widthAndHeight)
			: this(widthAndHeight, widthAndHeight)
		{
		}

		public Size(float width, float height)
		{
			Width = width;
			Height = height;
		}

		public Size(string sizeAsString)
		{
			var components = sizeAsString.SplitIntoFloats();
			if (components.Length != 2)
				throw new InvalidNumberOfDatatypeComponents<Size>();
			Width = components[0];
			Height = components[1];
		}

		public static readonly Size Zero;
		public static readonly Size One = new Size(1, 1);
		public static readonly Size Half = new Size(0.5f, 0.5f);
		public static readonly Size Quarter = new Size(0.25f, 0.25f);
		public static readonly Size Unused = new Size(-1, -1);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Size));

		[Pure] public float Width { get; }

		[Pure] public float Height { get; }

		[Pure] public float AspectRatio => Width / Height;
		[Pure] public float Length => MathExtensions.Sqrt(Width * Width + Height * Height);

		[Pure]
		public static Size operator *(Size s1, Size s2)
		{
			return new Size(s1.Width * s2.Width, s1.Height * s2.Height);
		}

		[Pure]
		public static Size operator *(Size s1, Scale s2)
		{
			return new Size(s1.Width * s2.X, s1.Height * s2.Y);
		}

		[Pure]
		public static Size operator *(Size s, float f)
		{
			return new Size(s.Width * f, s.Height * f);
		}

		[Pure]
		public static Size operator *(float f, Size s)
		{
			return new Size(f * s.Width, f * s.Height);
		}

		[Pure]
		public static Size operator /(Size s, float f)
		{
			return new Size(s.Width / f, s.Height / f);
		}

		[Pure]
		public static Size operator /(float f, Size s)
		{
			return new Size(f / s.Width, f / s.Height);
		}

		[Pure]
		public static Size operator /(Size s1, Size s2)
		{
			return new Size(s1.Width / s2.Width, s1.Height / s2.Height);
		}

		[Pure]
		public static Size operator /(Size s1, Scale s2)
		{
			return new Size(s1.Width / s2.X, s1.Height / s2.Y);
		}

		[Pure]
		public static Size operator +(Size s1, Size s2)
		{
			return new Size(s1.Width + s2.Width, s1.Height + s2.Height);
		}

		[Pure]
		public static Size operator -(Size s1, Size s2)
		{
			return new Size(s1.Width - s2.Width, s1.Height - s2.Height);
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Size ? Equals((Size) other) : base.Equals(other);
		}

		[Pure]
		public bool Equals(Size other)
		{
			return Width.IsNearlyEqual(other.Width, MathExtensions.Epsilon * 10) &&
			       Height.IsNearlyEqual(other.Height, MathExtensions.Epsilon * 10);
		}

		[Pure]
		public static bool operator ==(Size s1, Size s2)
		{
			return s1.Equals(s2);
		}

		[Pure]
		public static bool operator !=(Size s1, Size s2)
		{
			return !s1.Equals(s2);
		}

		[Pure]
		public static explicit operator Size(Vector2D vector2D)
		{
			return new Size(vector2D.X, vector2D.Y);
		}

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return Width.GetHashCode() * 31 + Height.GetHashCode();
			}
		}

		[Pure]
		public override string ToString()
		{
			return Width.ToInvariantString() + ", " + Height.ToInvariantString();
		}
	}
}