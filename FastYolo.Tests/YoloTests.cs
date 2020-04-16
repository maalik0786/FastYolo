using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using FastYolo.Model;
using NUnit.Framework;
using static FastYolo.ImageConverter;
using static FastYolo.DrawSquare;

namespace FastYolo.Tests
{
	public class YoloTests
	{
#if WIN64
		private const string YoloServerDirectory = @"\\DeltaServer\Shared\yolo-v3-tiny\";
#else
		private const string YoloServerDirectory = "/home/dev/Documents/yolo-v3-tiny/";
#endif
		private const string DummyImageFilename = YoloServerDirectory + "DummyNutInput.png";
		private const string DummyImageOutputFilename = YoloServerDirectory + "DummyNutOutput.jpg";
		private const string YoloWeightsFilename = YoloServerDirectory + "yolov3-tiny_walnut.weights";
		private const string YoloConfigFilename = YoloServerDirectory + "yolov3-tiny_walnut.cfg";
		private const string YoloClassesFilename = YoloServerDirectory + "classes.names";

		private YoloWrapper yoloWrapper;

		[SetUp]
		public void Setup() => yoloWrapper = new YoloWrapper(YoloConfigFilename, YoloWeightsFilename, YoloClassesFilename);

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
			var array = Image2Byte(image);
			var items = yoloWrapper.Detect(array, 4,true);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," + item.Width + "," + item.Height + "]");
		}

		[Test]
		public void PassIntPtrForObjectTracking()
		{
			var colorData = BitmapToColorData(new Bitmap(DummyImageFilename));
			const int Channels = 4;
			var floatArrayPointer = ColorData2YoloFormat(colorData, Channels);
			var items = yoloWrapper.Track(floatArrayPointer,
				colorData.Width, colorData.Height, Channels);
			Marshal.FreeHGlobal(floatArrayPointer);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
			{
				Assert.That(item.Type == "walnut" && item.Shape == YoloItem.ShapeType.Circle, Is.True);
				Console.WriteLine("Frame: " + item.FrameId + " Shape: " + item.Shape + " Found:" +
					item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," +
					item.Width + "," + item.Height + "]");
			}
		}

		[Test]
		public void LoadColorDataForObjectDetection()
		{
			var colorData = BitmapToColorData(new Bitmap(DummyImageFilename));
			var items = yoloWrapper.Detect(colorData, 4);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
			DrawBoundingBox(colorData, yoloItems);
			SaveAsBitmap(colorData).Save(DummyImageOutputFilename);
		}

		[Test]
		public void DisposeYoloWrapper() => yoloWrapper.Dispose();

		[Test][Category("Slow")]
		public void LoadJpegFromRaspberryCamera()
		{
			const int Width = 1280;
			const int Height = 720;
			const int DisWidth = 1280;
			const int DisHeight = 720;
			const int FrameRate = 30;
			var ptr = yoloWrapper.GetRaspberryCameraImage(Width,Height,DisWidth,DisHeight, FrameRate);
#if WIN64
			Assert.That(ptr, Is.EqualTo((IntPtr) 0));
#elif LINUX64
			Assert.That(ptr, Is.Not.EqualTo(0));
#else
			throw new PlatformNotSupportedException();
#endif
		}
	}
}