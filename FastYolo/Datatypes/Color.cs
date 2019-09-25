using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FastYolo.Extensions;

namespace FastYolo.Datatypes
{
	/// <summary>
	///   Color with a byte per component (red, green, blue, alpha), also provides float properties.
	/// </summary>
	[DebuggerDisplay("Color(r={R}, g={G}, b={B}, a={A})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Color : IEquatable<Color>, LerpWithParent<Color>
	{
		public Color(byte r, byte g, byte b, byte a = 255)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public readonly byte R;
		public readonly byte G;
		public readonly byte B;
		public readonly byte A;

		/// <summary>
		///   Constructor when passing in integer values to make sure to not use the float version, but
		///   instead threat them as byte values, which is what the caller intended. Does not clamp!
		/// </summary>
		public Color(int r, int g, int b, int a = 255)
		{
			R = (byte) r;
			G = (byte) g;
			B = (byte) b;
			A = (byte) a;
		}

		public Color(float redValue, float greenValue, float blueValue, float alphaValue = 1.0f)
		{
			R = (byte) (redValue.Clamp(0.0f, 1.0f) * 255.5f);
			G = (byte) (greenValue.Clamp(0.0f, 1.0f) * 255.5f);
			B = (byte) (blueValue.Clamp(0.0f, 1.0f) * 255.5f);
			A = (byte) (alphaValue.Clamp(0.0f, 1.0f) * 255.5f);
		}

		public Color(string colorAsString)
		{
			var components = colorAsString.SplitAndTrim(',');
			if (components.Length != 4)
				throw new InvalidNumberOfDatatypeComponents<Color>();
			R = components[0].Convert<byte>();
			G = components[1].Convert<byte>();
			B = components[2].Convert<byte>();
			A = components[3].Convert<byte>();
		}

		public Color(Color color, float alphaValue)
			: this(color.RedValue, color.GreenValue, color.BlueValue, alphaValue)
		{
		}

		public Color(Color color, byte a)
			: this(color.R, color.G, color.B, a)
		{
		}

		[Pure] public float RedValue => R / 255.0f;
		[Pure] public float GreenValue => G / 255.0f;
		[Pure] public float BlueValue => B / 255.0f;
		[Pure] public float AlphaValue => A / 255.0f;
		public static readonly Color Black = new Color(0, 0, 0);
		public static readonly Color White = new Color(255, 255, 255);
		public static readonly Color TransparentBlack = CreateTransparent(Black);
		public static readonly Color TransparentWhite = CreateTransparent(White);
		public static readonly Color Blue = new Color(0, 0, 255);
		public static readonly Color Cyan = new Color(0, 255, 255);
		public static readonly Color Gray = new Color(128, 128, 128);
		public static readonly Color Green = new Color(0, 255, 0);
		public static readonly Color Orange = new Color(255, 165, 0);
		public static readonly Color Pink = new Color(255, 192, 203);
		public static readonly Color Purple = new Color(255, 0, 255);
		public static readonly Color Red = new Color(255, 0, 0);
		public static readonly Color Teal = new Color(0, 128, 128);
		public static readonly Color Yellow = new Color(255, 255, 0);
		public static readonly Color Brown = new Color(128, 64, 0);
		public static readonly Color CornflowerBlue = new Color(100, 149, 237);
		public static readonly Color LightBlue = new Color(0.65f, 0.795f, 1f);
		public static readonly Color VeryLightGray = new Color(200, 200, 200);
		public static readonly Color LightGray = new Color(165, 165, 165);
		public static readonly Color DarkGray = new Color(89, 89, 89);
		public static readonly Color VeryDarkGray = new Color(32, 32, 32);
		public static readonly Color DarkGreen = new Color(0, 100, 0);
		public static readonly Color Gold = new Color(255, 215, 0);
		public static readonly Color PaleGreen = new Color(152, 251, 152);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Color));

		public static Color CreateTransparent(Color color)
		{
			return new Color(color, 0);
		}

		/// <summary>
		///   Colors are stored as RGBA byte values and this gives back the usual RGBA format as an
		///   optimized 32 bit value. r takes the first 8 bits, g the next 8 up to a for the last 8 bits.
		/// </summary>
		[Pure]
		public int PackedRgba => R + (G << 8) + (B << 16) + (A << 24);

		/// <summary>
		///   Instead of comparing a color to Color.White, use this much faster property.
		/// </summary>
		[Pure]
		public bool IsWhite => R == 255 && G == 255 && B == 255 && A == 255;

		/// <summary>
		///   Might be more useful in HSV space (but slower):
		///   http://stackoverflow.com/questions/13488957/interpolate-from-one-color-to-another
		/// </summary>
		[Pure]
		public Color Lerp(Color other, float interpolation)
		{
			return new Color((byte) (MathExtensions.Lerp(R, other.R, interpolation) + 0.25f),
				(byte) (MathExtensions.Lerp(G, other.G, interpolation) + 0.25f),
				(byte) (MathExtensions.Lerp(B, other.B, interpolation) + 0.25f),
				(byte) (MathExtensions.Lerp(A, other.A, interpolation) + 0.25f));
		}

		[Pure]
		public Color Lerp(Color other, float interpolation, Color parentColor)
		{
			return Lerp(other, interpolation) * parentColor;
		}

		/// <summary>
		///   Converts a percentage (0.0 to 1.0) into a color of the rainbow. Used to automatically
		///   assign unique colors to graph lines or for fancy fractal colors.
		/// </summary>
		public static Color CreateHeatmapColor(float percent)
		{
			if (0.0f <= percent && percent <= 0.125f)
				return new Color(0.0f, 0.0f, 4.0f * percent + .5f);
			if (0.125f < percent && percent <= 0.375f)
				return new Color(0.0f, 4.0f * percent - .5f, 0.0f);
			if (0.375f < percent && percent <= 0.625f)
				return new Color(4.0f * percent - 1.5f, 1.0f, -4.0f * percent + 2.5f);
			if (0.625f < percent && percent <= 0.875f)
				return new Color(1.0f, -4.0f * percent + 3.5f, 0.0f);
			return 0.875f < percent && percent <= 1.0f
				? new Color(-4.0f * percent + 4.5f, 0.0f, 0.0f)
				: White;
		}

		[Pure]
		public static bool operator !=(Color c1, Color c2)
		{
			return !c1.Equals(c2);
		}

		[Pure]
		public static bool operator ==(Color c1, Color c2)
		{
			return c1.Equals(c2);
		}

		[Pure]
		public bool Equals(Color other)
		{
			return PackedRgba == other.PackedRgba;
		}

		[Pure]
		public override bool Equals(object other)
		{
			return other is Color ? Equals((Color) other) : base.Equals(other);
		}

		[Pure]
		public override int GetHashCode()
		{
			return PackedRgba;
		}

		[Pure]
		public static Color operator *(Color c, float multiplier)
		{
			return new Color(c.RedValue * multiplier, c.GreenValue * multiplier, c.BlueValue * multiplier,
				c.AlphaValue * multiplier);
		}

		[Pure]
		public static Color operator *(Color c1, Color c2)
		{
			return new Color(c1.RedValue * c2.RedValue, c1.GreenValue * c2.GreenValue,
				c1.BlueValue * c2.BlueValue, c1.AlphaValue * c2.AlphaValue);
		}

		public static implicit operator System.Drawing.Color(Color v)
		{
			throw new NotImplementedException();
		}

		public static byte[] CreateRgbaBytesFromArray(Color[] colors)
		{
			var bytes = new byte[colors.Length * 4];
			for (var num = 0; num < colors.Length; num++)
			{
				bytes[num * 4 + 0] = colors[num].R;
				bytes[num * 4 + 1] = colors[num].G;
				bytes[num * 4 + 2] = colors[num].B;
				bytes[num * 4 + 3] = colors[num].A;
			}

			return bytes;
		}

		[Pure]
		public override string ToString()
		{
			return R + ", " + G + ", " + B + ", " + A;
		}
	}
}