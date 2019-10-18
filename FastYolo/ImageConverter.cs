using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FastYolo.Model;

namespace FastYolo
{
	public class ImageConverter
	{
		private readonly YoloObjectTypeResolver objectTypeResolver;
		public ImageConverter(YoloObjectTypeResolver yoloObjectTypeResolver) => objectTypeResolver = yoloObjectTypeResolver;

		public IEnumerable<YoloItem> Convert(BboxContainer container)
		{
			return container.candidates.Where(o => o.h > 0 || o.w > 0)
				.Select(item => new YoloItem
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
				}).ToList();
		}

		public ColorData BitmapToColorData(Bitmap image)
		{
			var colorData = new ColorData
			{
				Width = image.Width,
				Height = image.Height,
				Colors = new Color[image.Width * image.Height]
			};

			var bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
				ImageLockMode.ReadWrite, image.PixelFormat);
			unsafe
			{
				var p = (byte*) bmpData.Scan0.ToPointer();
				for (var x = 0; x < image.Width * image.Height; x++)
				{
					var r = *p++;
					var g = *p++;
					var b = *p++;
					var a = *p++;
					colorData.Colors[x] = new Color(r, g, b, a);
				}

				image.UnlockBits(bmpData);
			}

			return colorData;
		}

		// ReSharper disable once TooManyDeclarations
		public unsafe IntPtr ColorDataToYoloRgbFormat(ColorData imageData, int channels)
		{
			var sizeInBytes = imageData.Width * imageData.Height * channels * sizeof(float);
			var floatArrayPointer = Marshal.AllocHGlobal(sizeInBytes);
			var destination = (float*) floatArrayPointer.ToPointer();
			// yolo needs the data in format like red all, green all, blue all.
			for (var channel = 0; channel < channels; channel++)
			for (var y = 0; y < imageData.Height; y++)
			for (var x = 0; x < imageData.Width; x++)
			{
				var color = imageData.Colors[x + y * imageData.Width];

				*destination++ = channel switch
				{
					0 => color.RedValue,
					1 => color.GreenValue,
					2 => color.BlueValue,
					_ => color.AlphaValue
				};
			}

			return floatArrayPointer;
		}

		public unsafe IntPtr ToYoloRgbFormat(ColorData colorData, int yoloWidth = 416,
			int yoloHeight = 416, int channels = 3)
		{
			var sizeInBytes = yoloWidth * yoloHeight * channels * sizeof(float);
			var floatArrayPointer = Marshal.AllocHGlobal(sizeInBytes);
			var destination = (float*) floatArrayPointer.ToPointer();

			for (var channel = 0; channel < channels; channel++)
			for (var y = 0; y < yoloHeight; y++)
			for (var x = 0; x < yoloWidth; x++)
			{
				var imageX = x * colorData.Width / yoloWidth;
				var imageY = y * colorData.Height / yoloHeight;
				var color = colorData.Colors[imageX + imageY * colorData.Width];
				*destination++ = channel switch
				{
					0 => color.RedValue,
					1 => color.GreenValue,
					_ => color.BlueValue
				};
			}
			return floatArrayPointer;
		}

		public Image Byte2Image(byte[] imageData)
		{
			using var memoryStream = new MemoryStream(imageData);
			return Image.FromStream(memoryStream);
		}

		public byte[] Image2Byte(Image image)
		{
			using var memoryStream = new MemoryStream();
			image.Save(memoryStream, ImageFormat.Bmp);
			return memoryStream.ToArray();
		}
	}
}