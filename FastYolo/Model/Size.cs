using System;
using System.Diagnostics;
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
		public Size(float width, float height)
		{
			Width = width;
			Height = height;
		}

		public float Width { get; }
		public float Height { get; }

		public bool Equals(Size other) =>
			// ReSharper disable once ThrowExceptionInUnexpectedLocation
			throw new NotImplementedException(); //ncrunch: no coverage
	}
}