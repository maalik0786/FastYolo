using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FastYolo.Model;
using static FastYolo.ImageAnalyzer;
using static FastYolo.ImageConverter;
using Size = FastYolo.Model.Size;

namespace FastYolo
{
	// ReSharper disable once HollowTypeName
	public sealed class YoloWrapper : IDisposable
	{
		private readonly YoloObjectTypeResolver objectTypeResolver;
		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		public string? GraphicDeviceName { get; set; }
		public const int MaxObjects = 10;

		public YoloWrapper(string configurationFilename, string weightsFilename,
			string namesFilename, int gpu = 0)
		{
			objectTypeResolver = new YoloObjectTypeResolver(namesFilename);
			Initialize(configurationFilename, weightsFilename, gpu);
		}

#if WIN64
		private const string YoloGpuDllFilename = "yolo_cpp_dll.dll";
		private const string YoloPThreadDllFilename = "pthreadVC2.dll";
		private const string CudnnDllFilename = "cudnn64_8.dll";
		private const string CudnnRequiredDependencyFilename = "cudnn_ops_infer64_8.dll";
#if DEBUG
		private const string OpenCvWorldDllFilename = "opencv_world440d.dll";
#else
		private const string OpenCvWorldDllFilename = "opencv_world440.dll";
#endif
#elif LINUX64
		private const string YoloGpuDllFilename = "libdarknet_amd.so";
		private const string YoloPThreadDllFilename = "libpthread_amd.so";
		private const string OpenCvWorldDllFilename = "libopencv_world.so";
#else
		private const string YoloGpuDllFilename = "libdarknet_arm.so";
		private const string YoloPThreadDllFilename = "libpthread_arm.so";
		private const string OpenCvWorldDllFilename = "libopencv_world.so";
#endif

		[DllImport(YoloGpuDllFilename, EntryPoint = "init")]
		private static extern int InitializeYoloGpu(string configurationFilename,
			string weightsFilename, int gpu, int batchSize = 1);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_image")]
		private static extern int DetectImageGpu(string filename, ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_image")]
		// ReSharper disable TooManyArguments
		private static extern IntPtr GetRaspberryCameraJpegImage(int capWidth, int capHeight,
			int disWidth, int disHeight, int frameRate, int flip);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_objects")]
		private static extern int DetectObjectsGpu(IntPtr pArray, int width, int height,
			int channel, ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "track_objects")]
		private static extern int TrackObjectsGpu(IntPtr pArray, int width, int height,
			int channel, ref BboxContainer container);

		/// <summary>
		/// Resizing images every frame is VERY slow and should be avoided, use the given size from the
		/// configuration file passed into the constructor here (usually 416x416), resizing should not be
		/// done dynamically in the darknet library, extra memory would be required and it is obviously
		/// slower than if we already have 416x416 data!
		/// </summary>
		// ReSharper disable UnthrowableException
		[DllImport(YoloGpuDllFilename, EntryPoint = "CheckIfImageWasResized")]
		public static extern bool CheckIfImageWasResized();

		[DllImport(YoloGpuDllFilename, EntryPoint = "GetDetectorNetworkWidth")]
		public static extern int GetDetectorNetworkWidth();

		[DllImport(YoloGpuDllFilename, EntryPoint = "GetDetectorNetworkHeight")]
		public static extern int GetDetectorNetworkHeight();

		public void Dispose() => DisposeYoloGpu();

		[DllImport(YoloGpuDllFilename, EntryPoint = "dispose")]
		private static extern int DisposeYoloGpu();

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_device_count")]
		private static extern int GetDeviceCount();

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_device_name")]
		private static extern int GetDeviceName(int gpu, StringBuilder deviceName);

		// ReSharper disable once MethodTooLong
		private void Initialize(string configurationFilename, string weightsFilename, int gpu = 0)
		{
			if (IntPtr.Size != 8)
				throw new NotSupportedException("Only 64-bit processes are supported");
			const string CudaError = "An Nvidia GPU and CUDA 11.1 need to be installed! Please install CUDA " +
				"https://developer.nvidia.com/cuda-downloads\nError details: ";
#if WIN64
			if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUDA_PATH")))
				throw new DllNotFoundException(CudaError +
					"CUDA_PATH environment variable is not available!");
			if (!File.Exists(Path.Combine(Environment.GetEnvironmentVariable("CUDA_PATH")!, "bin",
				"CUDART64_110.DLL")))
				throw new DllNotFoundException(CudaError +
					@"cudart64_110.dll wasn't found in the CUDA_PATH\bin folder " +
					"(did you maybe install CUDA 10.* and not CUDA 11.1, " +
					"please install it again or fix your CUDA_PATH)");
			if (!File.Exists(Path.Combine(Environment.SystemDirectory, "NVCUDA.DLL")))
				throw new DllNotFoundException(CudaError +
					"NVCUDA.DLL wasn't found in the windows system directory, " +
					"is CUDA and your Nvidia graphics driver correctly installed?");
			if (!File.Exists(Path.Combine(Environment.GetEnvironmentVariable("CUDA_PATH")!, "bin",
					CudnnRequiredDependencyFilename)) &&
				(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUDNN")) || !File.Exists(
					Path.Combine(Environment.GetEnvironmentVariable("CUDNN")!, "bin",
						CudnnRequiredDependencyFilename))))
				throw new DllNotFoundException(CudaError +
					"CudNN dependencies not in CUDA_PATH and CUDNN environment variable is not available, make " +
					"sure CudNN 8 for Cuda 11.1+ is installed as well: https://developer.nvidia.com/rdp/cudnn-download");
			if (!File.Exists(CudnnDllFilename))
				throw new FileNotFoundException("Can't find the " + CudnnDllFilename);
#else
			if (!Directory.Exists("/usr/local/cuda"))
				throw new DllNotFoundException(CudaError + "CUDA is not available!");
#endif
			if (!File.Exists(OpenCvWorldDllFilename))
				throw new FileNotFoundException("Can't find the " + OpenCvWorldDllFilename);
			if (!File.Exists(YoloGpuDllFilename))
				throw new FileNotFoundException("Can't find the " + YoloGpuDllFilename);
			if (!File.Exists(YoloPThreadDllFilename))
				throw new FileNotFoundException("Can't find the " + YoloPThreadDllFilename);
			var deviceCount = GetDeviceCount();
			if (deviceCount == 0)
				throw new NotSupportedException("No graphic device is available");
			if (gpu > deviceCount - 1)
				throw new IndexOutOfRangeException("Graphic device index is out of range");
			var deviceName = new StringBuilder();
			GetDeviceName(gpu, deviceName);
			GraphicDeviceName = deviceName.ToString();
			InitializeYoloGpu(configurationFilename, weightsFilename, gpu);
		}

		public IntPtr GetRaspberryCameraImage(int capWidth, int capHeight, int disWidth,
			int disHeight, int frameRate, int flip = 0) =>
			GetRaspberryCameraJpegImage(capWidth, capHeight, disWidth, disHeight, frameRate, flip);

		public IEnumerable<YoloItem> Detect(string filepath)
		{
			if (!File.Exists(filepath))
				throw new FileNotFoundException("Cannot find the file", filepath);
			var container = new BboxContainer();
			DetectImageGpu(filepath, ref container);
			return Convert(container, objectTypeResolver);
		}

		public IEnumerable<YoloItem> Detect(ColorImage colorImage, int channels = 3,
			bool track = false) =>
			track
				? Track(ConvertColorImageToYoloFormat(colorImage, channels), colorImage.Width,
					colorImage.Height, channels)
				: Detect(ConvertColorImageToYoloFormat(colorImage, channels), colorImage.Width,
					colorImage.Height, channels);

		public IEnumerable<YoloItem> Detect(byte[] byteData, int channels = 3, bool track = false)
		{
			if (!IsValidImageFormat(byteData))
				throw new Exception("Invalid image data, wrong image format");
			using var image = (Bitmap) Byte2Image(byteData);
			var imageData = BitmapToColorImage(image, new Size(image.Width, image.Height));
			if (track)
				return Track(ConvertColorImageToYoloFormat(imageData, channels), imageData.Width,
					imageData.Height, channels);
			return Detect(ConvertColorImageToYoloFormat(imageData, channels), imageData.Width,
				imageData.Height, channels);
		}

		public IEnumerable<YoloItem> Detect(IntPtr floatArrayPointer, int width, int height, int channels = 3)
		{
			var container = new BboxContainer();
			DetectObjectsGpu(floatArrayPointer, width, height, channels, ref container);
			Marshal.FreeHGlobal(floatArrayPointer);
			return Convert(container, objectTypeResolver);
		}

		public IEnumerable<YoloItem> Track(IntPtr floatArrayPointer, int width, int height,
			int channel = 3)
		{
			var container = new BboxContainer();
			TrackObjectsGpu(floatArrayPointer, width, height, channel, ref container);
			Marshal.FreeHGlobal(floatArrayPointer);
			return Convert(container, objectTypeResolver);
		}
	}
}