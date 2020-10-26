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
		//public ColorImage(in int width, in int height) : this(new Size(width, height)) {}

		public ColorImage(in Size size)
		{
			Size = size;
			Colors = new Color[Width * Height];
		}

		private Size Size { get; }
		public Color[] Colors { get; set; }
		public int Width => (int)Size.Width;
		public int Height => (int)Size.Height;

		/*public ColorImage(Size size, Color[] colors)
		{
			Size = size;
			Colors = colors;
			if (colors.Length != Width * Height)
				throw new InvalidNumberOfColorsMustMatchWidthTimesHeight();
		}

		public class InvalidNumberOfColorsMustMatchWidthTimesHeight : Exception { }

		public void Resize(in Size newSize)
		{
			if (newSize.Width <= Width && newSize.Height <= Height)
				ResizeToSmaller(newSize);
			else
				ResizeToBigger(newSize);
		}

		private void ResizeToSmaller(in Size newSize)
		{
			var oldWidth = Width;
			var oldHeight = Height;
			Size = newSize;
			for (int y=0; y<Height; y++)
			for (int x = 0; x < Width; x++)
				Colors[y * Width + x] = Colors[
					(int)((y / newSize.Height) * oldHeight) * oldWidth +
					(int)((x / newSize.Width) * oldWidth)];
		}

		private void ResizeToBigger(in Size newSize)
		{
			var oldWidth = Width;
			var oldHeight = Height;
			var oldColors = Colors;
			Size = newSize;
			Colors = new Color[Width * Height];
			for (int y=0; y<Height; y++)
			for (int x = 0; x < Width; x++)
				Colors[y * Width + x] = oldColors[
					(int)(y / newSize.Height * oldHeight) * oldWidth + (int)(x / newSize.Width * oldWidth)];
		}*/
	}
}