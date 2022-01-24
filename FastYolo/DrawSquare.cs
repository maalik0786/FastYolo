using FastYolo.Model;

namespace FastYolo;

public sealed class DrawSquare
{
	private static Color drawColor = Color.Red;
	public void SetColor(Color color) => drawColor = color;

	public static void DrawBoundingBox(ColorImage colorImage, IEnumerable<YoloItem> items)
	{
		foreach (var item in items)
			DrawObjectFrame(colorImage, item.X, item.Y, item.Width, item.Height);
	}

	// ReSharper disable once TooManyArguments
	public static void DrawObjectFrame(ColorImage colorImage, int x, int y, int width,
		int height)
	{
		if (x == 0 || y == 0 || width == 0 || height == 0)
			return;
		DrawHorizontalLine(colorImage, x, y, width);
		DrawHorizontalLine(colorImage, x, y + height, width);
		DrawVerticalLine(colorImage, x, y, height);
		DrawVerticalLine(colorImage, x + width, y, height);
	}

	private static void DrawHorizontalLine(ColorImage colorImage, int x, int y, int width)
	{
		var colorPos = x + y * colorImage.Width;
		if (colorPos < 0)
			colorPos = 0; //ncrunch: no coverage end
		var stopBytePos = colorPos + width;
		if (stopBytePos > colorImage.Colors.Length)
			stopBytePos = colorImage.Colors.Length; //ncrunch: no coverage end
		while (colorPos < stopBytePos)
			colorImage.Colors[colorPos++] = drawColor;
	}

	private static void DrawVerticalLine(ColorImage colorImage, int x, int y, int height)
	{
		var colorPos = x + y * colorImage.Width;
		if (colorPos < 0)
			colorPos = 0; //ncrunch: no coverage end
		var stopBytePos = colorPos + height * colorImage.Width;
		if (stopBytePos > colorImage.Colors.Length)
			stopBytePos = colorImage.Colors.Length; //ncrunch: no coverage end
		while (colorPos < stopBytePos)
		{
			colorImage.Colors[colorPos++] = drawColor;
			colorPos += colorImage.Width - 1;
		}
	}
}