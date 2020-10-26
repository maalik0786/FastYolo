using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace FastYolo.Model
{
	/// <summary>
	/// Holds the width and height of an object (e.g. a rectangle or the screen resolution). This does
	/// not implements LerpWithParent on purpose, use Scale for scaling RenderData and affecting all
	/// children. SizeComponent holds the size for 2D entities and detects changes for optimizations.
	/// </summary>
	[DebuggerDisplay("Size({Width}, {Height})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public readonly struct Size : IEquatable<Size>
	{
		//public Size(float widthAndHeight) : this(widthAndHeight, widthAndHeight) { }

		public Size(float width, float height)
		{
			Width = width;
			Height = height;
		}

		/*public static readonly Size Zero;
		public static readonly Size One = new Size(1, 1);
		public static readonly Size Half = new Size(0.5f, 0.5f);
		public static readonly Size Quarter = new Size(0.25f, 0.25f);
		public static readonly Size Unused = new Size(-1, -1);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Size));*/

		[Pure]
		public float Width { get; }
		[Pure]
		public float Height { get; }
		
		/*[Pure]
		public float AspectRatio => Width / Height;

		[Pure]
		public static Size operator *(Size s1, Size s2)
			=> new Size(s1.Width * s2.Width, s1.Height * s2.Height);

		[Pure]
		public static Size operator *(Size s, float f) => new Size(s.Width * f, s.Height * f);

		[Pure]
		public static Size operator *(float f, Size s) => new Size(f * s.Width, f * s.Height);

		[Pure]
		public static Size operator /(Size s, float f) => new Size(s.Width / f, s.Height / f);

		[Pure]
		public static Size operator /(float f, Size s) => new Size(f / s.Width, f / s.Height);

		[Pure]
		public static Size operator /(Size s1, Size s2)
			=> new Size(s1.Width / s2.Width, s1.Height / s2.Height);

		[Pure]
		public static Size operator +(Size s1, Size s2)
			=> new Size(s1.Width + s2.Width, s1.Height + s2.Height);

		[Pure]
		public static Size operator -(Size s1, Size s2)
			=> new Size(s1.Width - s2.Width, s1.Height - s2.Height);*/

		public bool Equals(Size other) => throw new NotImplementedException(); //ncrunch: no coverage 

		/*[Pure]
		public override bool Equals(object? other)
			=> other is Size size ? Equals(size) : base.Equals(other);

		[Pure]
		public static bool operator ==(Size s1, Size s2) => s1.Equals(s2);

		[Pure]
		public static bool operator !=(Size s1, Size s2) => !s1.Equals(s2);

		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				return Width.GetHashCode() * 31 + Height.GetHashCode();
			}
		}*/
	}
}