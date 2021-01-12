using System;

namespace FastYolo.Model
{
	/// <summary>
	/// Provides width and height and colors as a plain array [y * Width + x], each color is 4 bytes
	/// (rgba) and immutable (like all structs), assign a new color if you need to change a pixel.
	/// Performance is better with plain arrays opposed to using multidimensional or nested arrays.
	/// Access is faster with 4 instead of 3 bytes (which is more compact, but not 32bit boundaries).
	/// </summary>
	public class ColorImage
	{
		public ColorImage(in int width, in int height)
        {
            Width = width;
            Height = height;
			Colors = new Color[Width * Height];
		}

		//public Size Size { get; private set; }
		public Color[] Colors { get; set; }
		public int Width;
		public int Height;

		//public ColorImage(Size size, Color[] colors)
		//{
		//	Size = size;
		//	Colors = colors;
		//	if (colors.Length != Width * Height)
		//		throw new InvalidNumberOfColorsMustMatchWidthTimesHeight();
		//}

		//public class InvalidNumberOfColorsMustMatchWidthTimesHeight : Exception { }

		public void Resize(in int width, in int height)
		{
			if (width <= Width && height <= Height)
				ResizeToSmaller(width, height);
			else
				ResizeToBigger(width, height);
		}

		private void ResizeToSmaller(in int width, in int height)
		{
			var oldWidth = Width;
			var oldHeight = Height;
			for (int y=0; y<Height; y++)
			for (int x = 0; x < Width; x++)
				Colors[y * Width + x] = Colors[
					(int)((y / height) * oldHeight) * oldWidth +
					(int)((x / width) * oldWidth)];
		}

		private void ResizeToBigger(in int width, in int height)
		{
			var oldWidth = Width;
			var oldHeight = Height;
			var oldColors = Colors;
			Colors = new Color[Width * Height];
			for (int y=0; y<Height; y++)
			for (int x = 0; x < Width; x++)
				Colors[y * Width + x] = oldColors[
					(int)(y / height * oldHeight) * oldWidth + (int)(x / width * oldWidth)];
		}
	}
}