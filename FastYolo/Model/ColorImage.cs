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

		public ColorImage(in Size size)
		{
			Size = size;
			Colors = new Color[Width * Height];
		}

		private Size Size { get; }
		public Color[] Colors { get; set; }
		public int Width => (int)Size.Width;
		public int Height => (int)Size.Height;
	}
}