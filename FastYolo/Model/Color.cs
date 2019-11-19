using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace FastYolo.Model
{
	/// <summary>
	///   Color with a byte per component (red, green, blue, alpha), also provides float properties.
	/// </summary>
	[DebuggerDisplay("Color(r={R}, g={G}, b={B}, a={A})")]
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct Color 
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

		public Color(Color color, byte a) : this(color.R, color.G, color.B, a) {}

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
		public static readonly Color VeryLightGray = new Color(200, 200, 200);
		public static readonly Color LightGray = new Color(165, 165, 165);
		public static readonly Color DarkGray = new Color(89, 89, 89);
		public static readonly Color VeryDarkGray = new Color(32, 32, 32);
		public static readonly Color DarkGreen = new Color(0, 100, 0);
		public static readonly Color Gold = new Color(255, 215, 0);
		public static readonly Color PaleGreen = new Color(152, 251, 152);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Color));

		public static Color CreateTransparent(Color color) => new Color(color, 0);
	}
}