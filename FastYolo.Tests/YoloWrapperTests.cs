using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FastYolo.Model;
using NUnit.Framework;
using static FastYolo.ImageConverter;
using Size = FastYolo.Model.Size;

namespace FastYolo.Tests
{
	/// <summary>
	/// If these tests are failing, first make sure the runtime is all in order by running FastYolo.TestApp,
	/// debug and fix all issues you see there before running these with NCrunch!
	/// </summary>
	public sealed class YoloWrapperTests
	{
		[SetUp]
		public void Setup() =>
			yolo = new YoloWrapper(YoloConfigurationTests.YoloConfigFilename,
				YoloConfigurationTests.YoloWeightsFilename,
				YoloConfigurationTests.YoloClassesFilename);

		private YoloWrapper yolo;

		[Test]
		public void LoadDummyImageForObjectDetection()
		{
			var items = yolo.Detect(YoloConfigurationTests.DummyImageFilename);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
		}

		[TearDown]
		public void KillYolo() => yolo.Dispose();

		[Test]
		public void ByteArrayForObjectDetection()
		{
			var image = Image.FromFile(YoloConfigurationTests.DummyImageFilename);
			Console.WriteLine("Width: " + image.Width);
			var array = Image2Byte(image);
			var items = yolo.Detect(array, 4, true);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," + item.Width + "," + item.Height + "]");
		}

		[Test]
		public unsafe void PassIntPtrForObjectTracking()
		{
			var image = new Bitmap(YoloConfigurationTests.DummyImageFilename, false);
			var colorImage = BitmapToColorImage(image, new Size(image.Width, image.Height));
			var floatArray = new FloatArray();
			var array = floatArray.GetYoloFloatArray(colorImage, Channels);
			IEnumerable<YoloItem> yoloResponse;
			fixed (float* floatArrayPointer = &array[0])
			{
				yoloResponse = yolo.Track(new IntPtr(floatArrayPointer), colorImage.Width,
					colorImage.Height, Channels);
			}
			foreach (var item in yoloResponse)
			{
				Assert.That(item.Type, Is.EqualTo("StressBall"));
				Console.WriteLine("Frame: " + item.FrameId + " Shape: " + item.Shape + " Found:" +
					item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," +
					item.Width + "," + item.Height + "]");
			}
		}

		private const int Channels = 4;

		[Test]
		public void LoadColorDataForObjectDetection()
		{
			var image = new Bitmap(YoloConfigurationTests.DummyImageFilename);
			var colorImage = BitmapToColorImage(image, new Size(image.Width, image.Height));
			var items = yolo.Detect(colorImage, Channels);
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
			var ptr = yolo.GetRaspberryCameraImage(Width, Height, DisWidth, DisHeight, FrameRate);
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