using System;
using System.Diagnostics;
using DeltaEngine.Logging;
using System.Drawing;
using System.Linq;
using FastYolo.Model;
using NUnit.Framework;
using static FastYolo.ImageConverter;
using static FastYolo.DrawSquare;
using System.Collections.Generic;

namespace FastYolo.Tests
{
	public class YoloTests
	{
		[SetUp]
		public void Setup()
		{
			yoloWrapper = new YoloWrapper(YoloConfigurationTests.YoloConfigFilename, YoloConfigurationTests.YoloWeightsFilename, YoloConfigurationTests.YoloClassesFilename);
			floatArray = new FloatArray();
		}
		private YoloWrapper yoloWrapper;
		private FloatArray floatArray;
		//
		
		[Test]
		public void LoadDummyImageForObjectDetection()
		{
			var items = yoloWrapper.Detect(YoloConfigurationTests.DummyImageFilename);
			var yoloItems = items as YoloItem[] ?? items.ToArray();
			Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
			foreach (var item in yoloItems)
				Debug.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
		}

		//[Test]
		//public void ByteArrayForObjectDetection()
		//{
		//	var image = Image.FromFile(YoloConfigurationTests.DummyImageFilename);
		//	width = image.Width;
		//	height = image.Height;
		//	var resizeImage = new ColorImage(width, height);
		//	resizeImage.Resize(416,416);
		//	Console.WriteLine("Width: "+ image.Width);
		//	var array = Image2Byte(resizeImage);
		//	var items = yoloWrapper.Detect(array, 4,true);
		//	var yoloItems = items as YoloItem[] ?? items.ToArray();
		//	Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
		//	foreach (var item in yoloItems)
		//		Console.WriteLine("Frame: " + item.FrameId + " Found:" + item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," + item.Width + "," + item.Height + "]");
		//}

		private ColorImage colorImage;


		[Test]
		public unsafe void PassIntPtrForObjectTracking()
		{
			var colorImage = BitmapToColorImage(new Bitmap(YoloConfigurationTests.DummyImageFilename, false));
		//	//width = colorImage.Width;
		//	//height = colorImage.Height;<PackageReference Include="Microsoft.NET.Test.Sdk" Version="16.4.0" />
		//	//var resizeImage = new ColorImage(width, height);
		//	//resizeImage.Resize(416,416);
			var array = floatArray.GetYoloFloatArray(colorImage, channels);
			IEnumerable<YoloItem> yoloResponse;
			fixed (float* floatArrayPointer = &array[0])
			{
				yoloResponse = yoloWrapper.Track(new IntPtr(floatArrayPointer), colorImage.Width,
					colorImage.Height, channels);
			}
			foreach (var item in yoloResponse)
			{
				Assert.That(item.Type == "walnut", Is.True);
				Console.WriteLine("Frame: " + item.FrameId + " Shape: " + item.Shape + " Found:" +
					item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," +
					item.Width + "," + item.Height + "]");
			}
		}

		//[Test]
		//public void PassIntPtrForObjectTracking()
		//{
		//	var colorImage = BitmapToColorImage(new Bitmap(YoloConfigurationTests.DummyImageFilename));
		//	width = colorImage.Width;
		//	height = colorImage.Height;
		//	var resizeImage = new ColorImage(width, height);
		//	resizeImage.Resize(416,416);
		//	var floatArrayPointer = ConvertColorDataToYoloFormat(resizeImage, Channels);
		//	var items = yoloWrapper.Track(floatArrayPointer,
		//		resizeImage.Width, resizeImage.Height, Channels);
		//	Marshal.FreeHGlobal(floatArrayPointer);
		//	var yoloItems = items as YoloItem[] ?? items.ToArray();
		//	Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
		//	foreach (var item in yoloItems)
		//	{
		//		Assert.That(item.Type == "walnut" && item.Shape == YoloItem.ShapeType.Circle, Is.True);
		//		Console.WriteLine("Frame: " + item.FrameId + " Shape: " + item.Shape + " Found:" +
		//			item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," +
		//			item.Width + "," + item.Height + "]");
		//	}
		//}

		private const int channels = 4;

		//[Test]
		//public void LoadColorDataForObjectDetection()
		//{
		//	var colorImage = BitmapToColorImage(new Bitmap(YoloConfigurationTests.DummyImageFilename));
		//	width = colorImage.Width;
		//	height = colorImage.Height;
		//	var items = yoloWrapper.Detect(colorImage, Channels);
		//	var yoloItems = items as YoloItem[] ?? items.ToArray();
		//	Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
		//	foreach (var item in yoloItems)
		//		Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
		//	//DrawBoundingBox(colorImage, yoloItems);
		//	//SaveAsBitmap(colorImage).Save(YoloConfigurationTests.DummyImageOutputFilename);
		//}

		//[TearDown]
		//public void DisposeYoloWrapper()
		//{
		//	Assert.That(YoloWrapper.CheckIfImageWasResized(), Is.False,
		//		"Slowdown because input image size: " + width + "x" + height +
		//		" is not in the same size as the configuration: " +
		//		YoloWrapper.GetDetectorNetworkWidth() + "x" +
		//		YoloWrapper.GetDetectorNetworkHeight());
		//	yoloWrapper.Dispose();
		//}

		//[Test][Category("Slow")]
		//public void LoadJpegFromRaspberryCamera()
		//{
		//	const int Width = 1280;
		//	const int Height = 720;
		//	const int DisWidth = 1280;
		//	const int DisHeight = 720;
		//	const int FrameRate = 30;
		//	var ptr = yoloWrapper.GetRaspberryCameraImage(Width,Height,DisWidth,DisHeight, FrameRate);
#if WIN64
		//	Assert.That(ptr, Is.EqualTo((IntPtr) 0));
#elif LINUX64
		//	Assert.That(ptr, Is.Not.EqualTo(0));
#else
		//	throw new PlatformNotSupportedException();
#endif
		//}
	}
}