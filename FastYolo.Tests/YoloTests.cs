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
		[SetUp]
		public void Setup() => yoloWrapper = new YoloWrapper(YoloConfigurationTests.YoloConfigFilename, YoloConfigurationTests.YoloWeightsFilename, YoloConfigurationTests.YoloClassesFilename);

		private YoloWrapper yoloWrapper;

		[Test]
		public void LoadDummyImageForObjectDetection()
		{
			var items = yoloWrapper.Detect(YoloConfigurationTests.DummyImageFilename);
			var image = Image.FromFile(YoloConfigurationTests.DummyImageFilename);
			width = image.Width;
			height = image.Height;
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
		}

		[Test]
		public void ByteArrayForObjectDetection()
		{
			var image = Image.FromFile(YoloConfigurationTests.DummyImageFilename);
			width = image.Width;
			height = image.Height;
			var array = Image2Byte(image);
			var items = yoloWrapper.Detect(array, 4,true);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," + item.Width + "," + item.Height + "]");
		}

		private int width;
		private int height;

		[Test]
		public void PassIntPtrForObjectTracking()
		{
			var colorData = BitmapToColorData(new Bitmap(YoloConfigurationTests.DummyImageFilename));
			width = colorData.Width;
			height = colorData.Height;
			var floatArrayPointer = ConvertColorDataToYoloFormat(colorData, Channels);
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

		private const int Channels = 4;

		[Test]
		public void LoadColorDataForObjectDetection()
		{
			var colorData = BitmapToColorData(new Bitmap(YoloConfigurationTests.DummyImageFilename));
			width = colorData.Width;
			height = colorData.Height;
			var items = yoloWrapper.Detect(colorData, Channels);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
			DrawBoundingBox(colorData, yoloItems);
			SaveAsBitmap(colorData).Save(YoloConfigurationTests.DummyImageOutputFilename);
		}

		[TearDown]
		public void DisposeYoloWrapper()
		{
			Assert.That(YoloWrapper.CheckIfImageWasResized(), Is.False,
				"Slowdown because input image size: " + width + "x" + height +
				" is not in the same size as the configuration: " +
				YoloWrapper.GetDetectorNetworkWidth() + "x" +
				YoloWrapper.GetDetectorNetworkHeight());
			yoloWrapper.Dispose();
		}

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