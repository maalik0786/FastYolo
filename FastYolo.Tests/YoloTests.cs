using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FastYolo.Model;
using Color = FastYolo.Datatypes.Color;

using NUnit.Framework;

namespace FastYolo.Tests
{
	public class YoloTests
	{

#if _WINDOWS
		private const string YoloServerDirectory = @"\\DeltaServer\Shared\yolo-v3-tiny\";
#else
		private const string YoloServerDirectory = "/home/dev/Documents/yolo-v3-tiny/";
#endif
		private const string DummyImageFilename = YoloServerDirectory + "DummyNutInput.png";
		private const string YoloWeightsFilename = YoloServerDirectory + "yolov3-tiny_walnut.weights";
		private const string YoloConfigFilename = YoloServerDirectory + "yolov3-tiny_walnut.cfg";
		private const string YoloClassesFilename = YoloServerDirectory + "classes.names";

		// ReSharper disable once InconsistentNaming
		private YoloWrapper yoloWrapper;

		[SetUp]
		public void Setup()
		{
			yoloWrapper = new YoloWrapper(YoloConfigFilename, YoloWeightsFilename, YoloClassesFilename);
		}

		[Test]
		public void LoadDummyImageForObjectDetection()
		{
			var items = yoloWrapper.Detect(DummyImageFilename);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
		}

		[Test]
		public void ByteArrayForObjectDetection()
		{
			var image = Image.FromFile(DummyImageFilename);
			var array = yoloWrapper.ToByteArray(image , ImageFormat.Png);
			var items = yoloWrapper.Detect(array, image.Width, image.Height, 4,true);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," + item.Width + "," + item.Height + "]");
		}

		[Test]
		public void PassIntPtrForObjectTracking()
		{
			var colorData = yoloWrapper.BitmapToColorData(new Bitmap(DummyImageFilename));
			const int Channels = 4;
			var items = yoloWrapper.Track(yoloWrapper.ColorDataToYoloRgbFormat(colorData, Channels), colorData.Width, colorData.Height, Channels);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " +
				                  item.TrackId + " BB: [" + item.X + "," +
				                  item.Y + "," + item.Width + "," + item.Height + "]");
		}

		[Test]
		public void LoadColorDataForObjectDetection()
		{
			var colorData = yoloWrapper.BitmapToColorData(new Bitmap(DummyImageFilename));
			var items = yoloWrapper.Detect(colorData, 4);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);		
		}

		[Test]
		public void LoadJpegFromRaspberryCamera()
		{
			const int Width = 1280;
			const int Height = 720;
			const int DisWidth = 1280;
			const int DisHeight = 720;
			const int FrameRate = 30;
			var ptr = yoloWrapper.GetRaspberryCameraImage(Width,Height,DisWidth,DisHeight, FrameRate);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Assert.That(ptr, Is.EqualTo((IntPtr) 0));
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				Assert.That(ptr, Is.Not.EqualTo(0));
			else
				throw new PlatformNotSupportedException();
		}
	}
}
