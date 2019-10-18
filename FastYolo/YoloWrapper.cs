using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FastYolo.Model;

namespace FastYolo
{
	public class YoloWrapper : IDisposable
	{
		private readonly ImageAnalyzer imageAnalyzer;
		private readonly ImageConverter imageConverter;

		public string GraphicDeviceName { get; private set; }
		public const int MaxObjects = 100;

		public YoloWrapper(string configurationFilename, string weightsFilename, string namesFilename,
			int gpu = 0)
		{
			imageAnalyzer = new ImageAnalyzer();
			imageConverter = new ImageConverter(new YoloObjectTypeResolver(namesFilename));
			Initialize(configurationFilename, weightsFilename, gpu);
		}

#if WIN64
		private const string YoloGpuDllFilename = "yolo_cpp_dll.dll";
		private const string YoloPThreadDllFilename = "pthreadVC2.dll";

#elif LINUX64
		private const string YoloGpuDllFilename = "libdarknet_amd.so";
		private const string YoloPThreadDllFilename = "libpthread_amd.so";
#else
		private const string YoloGpuDllFilename = "libdarknet_arm.so";
		private const string YoloPThreadDllFilename = "libpthread_arm.so";
		
#endif

		[DllImport(YoloGpuDllFilename, EntryPoint = "init")]
		public static extern int InitializeYoloGpu(string configurationFilename,
			string weightsFilename, int gpu);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_image")]
		public static extern int DetectImageGpu(string filename, ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_image")]
		public static extern IntPtr
			// ReSharper disable once TooManyArguments
			GetRaspberryCameraJpegImage(int capWidth, int capHeight, int disWidth, int disHeight,
				int frameRate, int flip);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_objects")]
		public static extern int
			// ReSharper disable once TooManyArguments
			DetectObjectsGpu(IntPtr pArray, int width, int height, int channel,
				ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "track_objects")]
		public static extern int
			// ReSharper disable once TooManyArguments
			TrackObjectsGpu(IntPtr pArray, int width, int height, int channel,
				ref BboxContainer container);

		public void Dispose() => DisposeYoloGpu();

		[DllImport(YoloGpuDllFilename, EntryPoint = "dispose")]
		public static extern int DisposeYoloGpu();

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_device_count")]
		public static extern int GetDeviceCount();

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_device_name")]
		public static extern int GetDeviceName(int gpu, StringBuilder deviceName);

		private void Initialize(string configurationFilename, string weightsFilename, int gpu = 0)
		{
			if (IntPtr.Size != 8)
				throw new NotSupportedException("Only 64-bit processes are supported");

			var cudaError =
				"An Nvidia GPU and CUDA 10.1 need to be installed! Please install CUDA " +
				"https://developer.nvidia.com/cuda-downloads\nError details: ";

#if WIN64
			if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUDA_PATH")))
				throw new DllNotFoundException(cudaError +
				                               "CUDA_PATH environment variable is not available!");
			if (!File.Exists(Path.Combine(Environment.GetEnvironmentVariable("CUDA_PATH"), "bin",
				"CUDART64_101.DLL")))
				throw new DllNotFoundException(cudaError +
				                               @"cudart64_101.dll wasn't found in the CUDA_PATH\bin folder " +
				                               "(did you maybe install CUDA 10.0 last and not CUDA 10.1, " +
				                               "please install it again or fix your CUDA_PATH)");
			if (!File.Exists(Path.Combine(Environment.SystemDirectory, "NVCUDA.DLL")))
				throw new DllNotFoundException(cudaError +
				                               "NVCUDA.DLL wasn't found in the windows system directory, " +
				                               "is CUDA and your Nvidia graphics driver correctly installed?");
#elif LINUX64
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				if (!Directory.Exists("/usr/local/cuda"))
					throw new DllNotFoundException(cudaError + "CUDA is not available!");
			}
#else
				throw new PlatformNotSupportedException();
#endif
			//ncrunch: no coverage start
			if (!File.Exists(YoloGpuDllFilename) || !File.Exists(YoloPThreadDllFilename))
				throw new FileNotFoundException("Can't find the " + YoloGpuDllFilename + " or " + YoloPThreadDllFilename);
			//ncrunch: no coverage end
			var deviceCount = GetDeviceCount();

			if (deviceCount == 0)
				throw new NotSupportedException("No graphic device is available");
			if (gpu > deviceCount - 1)
				throw new IndexOutOfRangeException("Graphic device index is out of range");
			var deviceName = new StringBuilder(); //allocate memory for string
			GetDeviceName(gpu, deviceName);
			GraphicDeviceName = deviceName.ToString();
			InitializeYoloGpu(configurationFilename, weightsFilename, gpu);
		}

		// ReSharper disable once TooManyArguments
		public IntPtr GetRaspberryCameraImage(int capWidth, int capHeight,
			int disWidth, int disHeight, int frameRate, int flip = 0)
		{
			return GetRaspberryCameraJpegImage(capWidth, capHeight, disWidth, disHeight, frameRate, flip);
		}

		public IEnumerable<YoloItem> Detect(string filepath)
		{
			if (!File.Exists(filepath))
				throw new FileNotFoundException("Cannot find the file", filepath);
			var container = new BboxContainer();
			DetectImageGpu(filepath, ref container);
			return imageConverter.Convert(container);
		}

		public IEnumerable<YoloItem> Detect(ColorData imageData,
			int channels = 3, bool track = false)
		{
			return track ? Track(imageConverter.ToYoloRgbFormat(imageData)) : Detect(imageConverter.ToYoloRgbFormat(imageData));
		}

		public IEnumerable<YoloItem> Detect(byte[] byteData, int width, int height,
			int channels = 3, bool track = false)
		{
			if (!imageAnalyzer.IsValidImageFormat(byteData))
				throw new Exception("Invalid image data, wrong image format");

			var imageData = imageConverter.BitmapToColorData((Bitmap) Image.FromStream(new MemoryStream(byteData)));
			return track
				? Track(imageConverter.ColorDataToYoloRgbFormat(imageData, channels), imageData.Width, imageData.Height,
					channels)
				: Detect(imageConverter.ColorDataToYoloRgbFormat(imageData, channels), imageData.Width, imageData.Height,
					channels);
		}

		// ReSharper disable once TooManyArguments
		public IEnumerable<YoloItem> Detect(IntPtr floatArrayPointer, int width = 416, int height = 416,
			int channels = 3)
		{
			var container = new BboxContainer();
			try
			{
				DetectObjectsGpu(floatArrayPointer, width, height, channels, ref container);
			}
			finally
			{
				Marshal.FreeHGlobal(floatArrayPointer);
			}

			return imageConverter.Convert(container);
		}

		// ReSharper disable once TooManyArguments
		public IEnumerable<YoloItem> Track(IntPtr floatArrayPointer, int width = 416, int height = 416,
			int channel = 3)
		{
			var container = new BboxContainer();
			try
			{
				TrackObjectsGpu(floatArrayPointer, width, height, channel, ref container);
			}
			finally
			{
				Marshal.FreeHGlobal(floatArrayPointer);
			}

			return imageConverter.Convert(container);
		}
	}
}