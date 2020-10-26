using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FastYolo.Model;
using NUnit.Framework;
using static FastYolo.ImageConverter;
using static FastYolo.DrawSquare;
using Size = FastYolo.Model.Size;

namespace FastYolo.Tests
{
	public class YoloTests
	{
		[SetUp]
		public void Setup()
		{
			yoloWrapper = new YoloWrapper(YoloConfigurationTests.YoloConfigFilename,
				YoloConfigurationTests.YoloWeightsFilename, YoloConfigurationTests.YoloClassesFilename);
			floatArray = new FloatArray();
			var image = new Bitmap(YoloConfigurationTests.DummyImageFilename);
			colorImage = BitmapToColorData(image, new Size(image.Width, image.Height));
		}

		private YoloWrapper yoloWrapper;
		private FloatArray floatArray;

		[Test]
		public void LoadDummyImageForObjectDetection()
		{
			var items = yoloWrapper.Detect(YoloConfigurationTests.DummyImageFilename);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
		}

		[Test]
		public void ByteArrayForObjectDetection()
		{
			var image = Image.FromFile(YoloConfigurationTests.DummyImageFilename);
			var array = Image2Byte(image);
			var items = yoloWrapper.Detect(array, 4, true);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," + item.Width + "," + item.Height + "]");
		}

		private ColorImage colorImage;

		[Test]
		public unsafe void PassIntPtrForObjectTracking()
		{
			var array = floatArray.GetYoloFloatArray(colorImage, Channels);
			IEnumerable<YoloItem> yoloResponse;
			if (YoloWrapper.CheckIfImageWasResized())
				throw new Exception("Slowdown because input image size: " + colorImage.Width + "x" +
														colorImage.Height + " is not in the same size as the configuration: " +
														YoloWrapper.GetDetectorNetworkWidth() + "x" + YoloWrapper.GetDetectorNetworkHeight());
			fixed (float* floatArrayPointer = &array[0])
				yoloResponse = yoloWrapper.Track(new IntPtr(floatArrayPointer), colorImage.Width,
					colorImage.Height,
					Channels);
			foreach (var item in yoloResponse)
			{
				Assert.That(item.Type == "walnut", Is.True);
				Console.WriteLine("Frame: " + item.FrameId + " Shape: " + item.Shape + " Found:" +
					item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," +
					item.Width + "," + item.Height + "]");
			}
		}

		private const int Channels = 4;

		[Test]
		public void LoadColorDataForObjectDetection()
		{
			var items = yoloWrapper.Detect(colorImage, Channels);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
			DrawBoundingBox(colorImage, yoloItems);
		}

		[TearDown]
		public void DisposeYoloWrapper() => yoloWrapper.Dispose();

		[Test]
		public void LoadJpegFromRaspberryCamera()
		{
			const int Width = 1280;
			const int Height = 720;
			const int DisWidth = 1280;
			const int DisHeight = 720;
			const int FrameRate = 30;
			var ptr = yoloWrapper.GetRaspberryCameraImage(Width, Height, DisWidth, DisHeight, FrameRate);
#if WIN64
			Assert.That(ptr, Is.EqualTo((IntPtr)0));
#elif LINUX64
					Assert.That(ptr, Is.Not.EqualTo(0));
#else
					throw new PlatformNotSupportedException();
#endif
		}
	}
}