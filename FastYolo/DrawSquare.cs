using FastYolo.Model;

namespace FastYolo;

public sealed class DrawSquare
{
	private static Color drawColor = Color.Red;
	public void SetColor(Color color) => drawColor = color;

	public static void DrawBoundingBox(ColorImage colorData, IEnumerable<YoloItem> items)
	{
		foreach (var item in items)
			DrawObjectFrame(colorData, item.X, item.Y, item.Width, item.Height);
	}

	// ReSharper disable once TooManyArguments
	public static void DrawObjectFrame(ColorImage vd, int x, int y, int width,
		int height)
	{
		if (x == 0 || y == 0 || width == 0 || height == 0)
			return;
		DrawHorizontalLine(vd, x, y, width);
		DrawHorizontalLine(vd, x, y + height, width);
		DrawVerticalLine(vd, x, y, height);
		DrawVerticalLine(vd, x + width, y, height);
	}

	private static void DrawHorizontalLine(ColorImage vd, int x, int y, int width)
	{
		var colorPos = x + y * vd.Width;
		if (colorPos < 0)
			colorPos = 0; //ncrunch: no coverage end
		var stopBytePos = colorPos + width;
		if (stopBytePos > vd.Colors.Length)
			stopBytePos = vd.Colors.Length; //ncrunch: no coverage end
		while (colorPos < stopBytePos)
			vd.Colors[colorPos++] = drawColor;
	}

	private static void DrawVerticalLine(ColorImage vd, int x, int y, int height)
	{
		var colorPos = x + y * vd.Width;
		if (colorPos < 0)
			colorPos = 0; //ncrunch: no coverage end
		var stopBytePos = colorPos + height * vd.Width;
		if (stopBytePos > vd.Colors.Length)
			stopBytePos = vd.Colors.Length; //ncrunch: no coverage end
		while (colorPos < stopBytePos)
		{
			vd.Colors[colorPos++] = drawColor;
			colorPos += vd.Width - 1;
		}
	}
}