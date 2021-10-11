﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FastYolo.Model;
using Color = FastYolo.Model.Color;
using Size = FastYolo.Model.Size;

namespace FastYolo
{
	public static class ImageConverter
	{
		public static IEnumerable<YoloItem>
			Convert(BboxContainer container, YoloObjectTypeResolver objectTypeResolver) =>
			container.candidates.Where(o => o.h > 0 || o.w > 0).Select(item => new YoloItem
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

		public static ColorImage BitmapToColorImage(Bitmap image, Size size)
		{
			var colorImage = new ColorImage(size);
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
					colorImage.Colors[x] = new Color(r, g, b, a);
				}
				image.UnlockBits(bmpData);
			}
			return colorImage;
		}

		public static unsafe IntPtr ConvertColorImageToYoloFormat(ColorImage colorData,
			int channels = 3)
		{
			var sizeInBytes = colorData.Width * colorData.Height * channels * sizeof(float);
			var floatArrayPointer = Marshal.AllocHGlobal(sizeInBytes);
			var destination = (float*) floatArrayPointer.ToPointer();
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
			return floatArrayPointer;
		}

		public static byte[] Image2Byte(Image image)
		{
			using var memoryStream = new MemoryStream();
			image.Save(memoryStream, ImageFormat.Bmp);
			return memoryStream.ToArray();
		}

		public static Image Byte2Image(byte[] byteData) =>
			Image.FromStream(new MemoryStream(byteData));
	}
}