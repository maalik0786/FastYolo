using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastYolo
{
	public static class ImageAnalyzer
	{
		private static readonly Dictionary<string, byte[]> ImageFormats = new();

		static ImageAnalyzer()
		{
			var bmp = Encoding.ASCII.GetBytes("BM"); //BMP
			var png = new byte[] {137, 80, 78, 71}; //PNG
			var jpeg = new byte[] {255, 216, 255}; //JPEG

			ImageFormats.Add("bmp", bmp);
			ImageFormats.Add("png", png);
			ImageFormats.Add("jpeg", jpeg);
		}

		public static bool IsValidImageFormat(byte[] imageData) =>
			imageData.Length > 3 && ImageFormats.Any(imageFormat =>
				imageData.Take(imageFormat.Value.Length).SequenceEqual(imageFormat.Value));
	}
}