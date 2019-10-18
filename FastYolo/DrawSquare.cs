using System.Collections.Generic;
using FastYolo.Model;
using Color = FastYolo.Color;
using ColorData = FastYolo.Model.ColorData;

namespace FastYolo
{
	public class DrawSquare
	{
		private Color drawColor = Color.Red;
		public void SetColor(Color color) => drawColor = color;

		public void DrawBoundingBox(ColorData colorData, IEnumerable<YoloItem> items)
		{
			if (items == null) return;
			foreach (var item in items) DrawObjectFrame(colorData, (int) item.X, (int) item.Y, (int) item.Width, (int) item.Height);
		}

		public void DrawObjectFrame(ColorData vd, int x, int y, int width, int height)
		{
			if (x == 0 || y == 0 || width == 0 || height == 0)
				return;
			DrawHorizontalLine(vd, x, y, width);
			DrawHorizontalLine(vd, x, y + height, width);
			DrawVerticalLine(vd, x, y, height);
			DrawVerticalLine(vd, x + width, y, height);
		}

		public void DrawHorizontalLine(ColorData vd, int x, int y, int width)
		{
			var colorPos = x + y * vd.Width;
			if (colorPos < 0) colorPos = 0;
			var stopBytePos = colorPos + width;
			if (stopBytePos > vd.Colors.Length) stopBytePos = vd.Colors.Length;
			while (colorPos < stopBytePos) vd.Colors[colorPos++] = drawColor;
		}

		public void DrawVerticalLine(ColorData vd, int x, int y, int height)
		{
			var colorPos = x + y * vd.Width;
			if (colorPos < 0) colorPos = 0;
			var stopBytePos = colorPos + height * vd.Width;
			if (stopBytePos > vd.Colors.Length) stopBytePos = vd.Colors.Length;
			while (colorPos < stopBytePos)
			{
				vd.Colors[colorPos++] = drawColor;
				colorPos += vd.Width - 1;
			}
		}
	}
}
