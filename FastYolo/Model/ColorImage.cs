namespace FastYolo.Model;

/// <summary>
/// Provides width and height and colors as a plain array [y * Width + x], each color is 4 bytes
/// (rgba) and immutable (like all structs), assign a new color if you need to change a pixel.
/// Performance is better with plain arrays opposed to using multidimensional or nested arrays.
/// Access is faster with 4 instead of 3 bytes (which is more compact, but not 32bit boundaries).
/// </summary>
public sealed class ColorImage
{
	public ColorImage(int width, int height)
	{
		Width = width;
		Height = height;
		Colors = new Color[width * height];
	}

	public ColorImage SetColors(Color[] colors)
	{
		Colors = colors;
		return this;
	}

	public Color[] Colors { get; set; }
	public int Width { get; }
	public int Height { get; }
}