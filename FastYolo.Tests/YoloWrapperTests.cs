using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FastYolo.Model;
using ManagedCuda;
using NUnit.Framework;
using static FastYolo.ImageConverter;
using static FastYolo.Tests.YoloConfigurationTests;

namespace FastYolo.Tests;

/// <summary>
/// If these tests are failing, first make sure the runtime is all in order by running FastYolo.TestApp,
/// debug and fix all issues you see there before running these with NCrunch!
/// </summary>
public sealed class YoloWrapperTests
{
	[SetUp]
	public void Setup()
	{
		yolo = new YoloWrapper(YoloConfigFilename, YoloWeightsFilename, YoloClassesFilename);
		var image = Image.FromFile(ImageFilename);
		channels = image.RawFormat.Equals(ImageFormat.Png)
			? 4
			: 3;
		byteArray = Image2Byte(image);
		colorImage = BitmapToColorImage((Bitmap) image, channels);
		var floatArray = new FloatArray();
		floatYoloFormatArray = floatArray.GetYoloFloatArray(colorImage, channels);
	}

	private YoloWrapper yolo;
	private byte[] byteArray;
	private static ColorImage colorImage;
	private float[] floatYoloFormatArray;
	private int channels;

	[Test]
	public void LoadDummyImageForObjectDetection()
	{
		var items = yolo.Detect(ImageFilename);
		var yoloItems = items as YoloItem[] ?? items.ToArray();
		Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
		foreach (var item in yoloItems)
			Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
	}

	[Test]
	public void ByteArrayWithOpencvMatFormatConversion()
	{
		var items = yolo.Detect(byteArray);
		var yoloItems = items as YoloItem[] ?? items.ToArray();
		Assert.That(yoloItems, Is.Not.Null.Or.InnerException);
		WriteDetectedObjectsOnConsole(yoloItems);
	}

	[TestCase(true)]
	[TestCase(false)]
	public unsafe void PassIntPtrFromFloatArray(bool track)
	{
		IEnumerable<YoloItem> yoloItems;
		fixed (float* floatArrayPointer = &floatYoloFormatArray[0])
		{
			yoloItems = track
				? yolo.Track(new IntPtr(floatArrayPointer), colorImage.Width, colorImage.Height,
					channels)
				: yolo.Detect(new IntPtr(floatArrayPointer), colorImage.Width, colorImage.Height,
					channels);
		}
		WriteDetectedObjectsOnConsole(yoloItems);
	}

	[Test]
	//[Category("Slow")]
	public unsafe void BenchmarkIntPtrMethod()
	{
		fixed (float* floatArrayPointer = &floatYoloFormatArray[0])
		{
			var arrayPointer = new IntPtr(floatArrayPointer);
			for (var index = 0; index < 100; index++)
				yolo.Detect(arrayPointer, colorImage.Width, colorImage.Height,
					channels);
		}
	}

	[TestCase(true)]
	[TestCase(false)]
	public unsafe void PassIntPtrFromColorImageConversion(bool track)
	{
		var sizeInBytes = colorImage.Width * colorImage.Height * channels * sizeof(float);
		var ptr = Marshal.AllocHGlobal(sizeInBytes);
		var pointer = (float*) ptr.ToPointer();
		ConvertColorImageToYoloFormat(colorImage, ref pointer);
		var yoloItems = track
			? yolo.Track(ptr, colorImage.Width, colorImage.Height, channels)
			: yolo.Detect(ptr, colorImage.Width, colorImage.Height, channels);
		WriteDetectedObjectsOnConsole(yoloItems);
		Marshal.FreeHGlobal(ptr);
	}

	[Test]
	public void GpuCudaDevicePointer()
	{
		var bitmap = Image.FromFile(ImageFilename.Replace(".jpg", " out.jpg"));
		var image = BitmapToColorImage((Bitmap) bitmap, channels);
		CudaDeviceVariable<float> gpuPointer = new float[image.Width * image.Height * 3];
		gpuPointer.CopyToDevice(floatYoloFormatArray);
		var yoloItems = yolo.DetectCuda(gpuPointer.DevicePointer.Pointer, image.Width,
			image.Height);
		WriteDetectedObjectsOnConsole(yoloItems);
		gpuPointer.Dispose();
	}

	private static void WriteDetectedObjectsOnConsole(IEnumerable<YoloItem> yoloResponse)
	{
		foreach (var item in yoloResponse)
			Console.WriteLine("Frame: " + item.FrameId + " Shape: " + item.Shape + " Found:" +
				item.Type + " ID: " + item.TrackId + " BB: [" + item.X + "," + item.Y + "," +
				item.Width + "," + item.Height + "]");
	}

	[TearDown]
	public void KillYolo() => yolo.Dispose();
}