using FastYolo.Datatypes;

namespace FastYolo.Model
{
	/// <summary>
	///   Provides width and height and colors as a plain array [y * width + x], each color is 4 bytes
	///   (rgba) and immutable (like all structs), assign a new color if you need to change a pixel.
	///   Performance is better with plain arrays opposed to using multidimensional or nested arrays.
	/// </summary>
	public class ColorData
	{
		public Color[]
			Colors; //TODO: we need a discussion on rgba vs bgra, which is the default for webcam, streams input and forms and wpf output, we need to convert 2 times, not sure if this is worth it if we can just swap r and b in code

		public int Height;

		public int Width;
	}
}