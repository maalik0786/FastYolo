namespace FastYolo.Model
{
	/// <summary>
	/// Provides width and height and colors as a plain array [y * width + x], each color is 4 bytes
	/// (rgba) and immutable (like all structs), assign a new color if you need to change a pixel.
	/// Performance is better with plain arrays opposed to using multidimensional or nested arrays.
	/// </summary>
	public class ColorData
	{
		public int Width;
		public int Height;
		public Color[] Colors;
	}
}