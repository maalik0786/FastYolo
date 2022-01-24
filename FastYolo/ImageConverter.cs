using System.Drawing;
using System.Drawing.Imaging;
using FastYolo.Model;

namespace FastYolo;

public static class ImageConverter
{
	public static IEnumerable<YoloItem>
		Convert(BboxContainer container, YoloObjectTypeResolver objectTypeResolver)
	{
		var yoloItems = new List<YoloItem>();
		foreach (var item in container.candidates)
			if (item.w > 0 || item.h > 0)
				yoloItems.Add(new YoloItem
				{
					X = (int) item.x,
					Y = (int) item.y,
					Height = (int) item.h,
					Width = (int) item.w,
					Confidence = item.prob,
					FrameId = (int) item.frames_counter,
					TrackId = (int) item.track_id,
					Shape = (YoloItem.ShapeType) item.shape,
					Type = objectTypeResolver.Resolve((int) item.obj_id)
				});
		return yoloItems;
	}

	public static ColorImage BitmapToColorImage(Bitmap image, int channels)
	{
		if (colorImage.Width != image.Width && colorImage.Height != image.Height)
		{
			colorImage = new ColorImage(image.Width, image.Height);
			rectangle = new Rectangle(0, 0, image.Width, image.Height);
		}
		var bmpData = image.LockBits(rectangle,
			ImageLockMode.ReadWrite, image.PixelFormat);
		unsafe
		{
			var p = (byte*) bmpData.Scan0.ToPointer();
			for (var x = 0; x < image.Width * image.Height; x++)
			{
				colorImage.Colors[x].R = *p++;
				colorImage.Colors[x].G = *p++;
				colorImage.Colors[x].B = *p++;
				if (channels is 4)
					colorImage.Colors[x].A = *p++;
			}
			image.UnlockBits(bmpData);
		}
		return colorImage;
	}

	private static ColorImage colorImage = new(0, 0);
	private static Rectangle rectangle = new(0, 0, colorImage.Width, colorImage.Height);

	public static unsafe void ConvertColorImageToYoloFormat(ColorImage colorData, ref float* destination,
		int channels = 3)
	{
		for (var channel = 0; channel < channels; channel++)
		for (var y = 0; y < colorData.Height; y++)
		for (var x = 0; x < colorData.Width; x++)
		{
			var color = colorData.Colors[x + y * colorData.Width];
			(*destination++) = channel switch
			{
				0 => color.RedValue,
				1 => color.GreenValue,
				2 => color.BlueValue,
				_ => color.AlphaValue
			};
		}
	}

	public static byte[] Image2Byte(Image image)
	{
		using var memoryStream = new MemoryStream();
		image.Save(memoryStream, image.RawFormat);
		return memoryStream.ToArray();
	}

	public static Image Byte2Image(byte[] byteData) =>
		Image.FromStream(new MemoryStream(byteData));
}