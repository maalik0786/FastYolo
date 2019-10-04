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

namespace FastYolo
{
	public class YoloWrapper : IDisposable
	{
		public YoloWrapper(string configurationFilename, string weightsFilename, string namesFilename,
			int gpu = 0)
		{
			Initialize(configurationFilename, weightsFilename, namesFilename, gpu);
		}

		private readonly ImageAnalyzer imageAnalyzer = new ImageAnalyzer();
		public const int MaxObjects = 100;

#if _WINDOWS
		private const string YoloGpuDllFilename = "yolo_cpp_dll.dll";
		private const string YoloPThreadDllFilename = "pthreadVC2.dll";

#elif _ARCHITECTURE
		private const string YoloGpuDllFilename = "libdarknet_amd.so";
		private const string YoloPThreadDllFilename = "libpthread_amd.so";
#else
		private const string YoloGpuDllFilename = "libdarknet_arm.so";
		private const string YoloPThreadDllFilename = "libpthread_arm.so";
		
#endif
		private YoloObjectTypeResolver objectTypeResolver;

		[DllImport(YoloGpuDllFilename, EntryPoint = "init")]
		internal static extern int InitializeYoloGpu(string configurationFilename,
			string weightsFilename, int gpu);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_image")]
		internal static extern int DetectImageGpu(string filename, ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_image")]
		public static extern IntPtr
			// ReSharper disable once TooManyArguments
			GetRaspberryCameraJpegImage(int capWidth, int capHeight, int disWidth, int disHeight,
				int frameRate, int flip);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_objects")]
		internal static extern int
			// ReSharper disable once TooManyArguments
			DetectObjectsGpu(IntPtr pArray, int width, int height, int channel,
				ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "detect_mat")]
		internal static extern int
			// ReSharper disable once TooManyArguments
			DetectObjects(IntPtr pArray, int width, int height, int channel, ref BboxContainer container);

		[DllImport(YoloGpuDllFilename, EntryPoint = "track_objects")]
		internal static extern int
			// ReSharper disable once TooManyArguments
			TrackObjectsGpu(IntPtr pArray, int width, int height, int channel,
				ref BboxContainer container);

		public void Dispose()
		{
			DisposeYoloGpu();
		}

		[DllImport(YoloGpuDllFilename, EntryPoint = "dispose")]
		internal static extern int DisposeYoloGpu();

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_device_count")]
		internal static extern int GetDeviceCount();

		[DllImport(YoloGpuDllFilename, EntryPoint = "get_device_name")]
		internal static extern int GetDeviceName(int gpu, StringBuilder deviceName);

		private void Initialize(string configurationFilename, string weightsFilename,
			string namesFilename, int gpu = 0)
		{
			if (IntPtr.Size != 8)
				throw new NotSupportedException("Only 64-bit processes are supported");

			// See readme.txt for all the steps we need to check here if CUDA 10.1 and CudNN64_7.dll are installed
			var cudaError =
				"An Nvidia GPU and CUDA 10.1 need to be installed! Please install CUDA " +
				"https://developer.nvidia.com/cuda-downloads\nError details: ";

#if _WINDOWS
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
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				if (!Directory.Exists("/usr/local/cuda"))
					throw new DllNotFoundException(cudaError + "CUDA is not available!");
			}
			else
				throw new PlatformNotSupportedException();
#endif
			if (!File.Exists(YoloGpuDllFilename1))
				throw new DllMissing(YoloGpuDllFilename1);
			if (!File.Exists(YoloPThreadDllFilename))
				throw new DllMissing(YoloPThreadDllFilename);

			int deviceCount;
			try
			{
				deviceCount = GetDeviceCount();
			}
			catch (DllNotFoundException ex)
			{
				throw new DllNotFoundException(ex.Message +
				                               " This means wrong dlls are used, not exactly CUDA 10.1 as needed and linked here");
			}

			if (deviceCount == 0)
				throw new NotSupportedException("No graphic device is available");
			if (gpu > deviceCount - 1)
				throw new IndexOutOfRangeException("Graphic device index is out of range");
			var deviceName = new StringBuilder(); //allocate memory for string
			GetDeviceName(gpu, deviceName);
			GraphicDeviceName = deviceName.ToString();
			InitializeYoloGpu(configurationFilename, weightsFilename, gpu);
			objectTypeResolver = new YoloObjectTypeResolver(namesFilename);
		}

		// ReSharper disable once TooManyArguments
		public IntPtr GetRaspberryCameraImage(int capWidth = 1280, int capHeight = 720,
			int disWidth = 1280, int disHeight = 720, int frameRate = 30, int flip = 0)
		{
			return GetRaspberryCameraJpegImage(capWidth, capHeight, disWidth, disHeight, frameRate, flip);
		}

		private class DllMissing : DllNotFoundException
		{
			public DllMissing(string dllFilename) : base(
				dllFilename +
				" wasn't found in the local path, it should have been automatically copied and installed, make sure to use the nuget package with the native binaries!")
			{
			}
		}

		public string GraphicDeviceName { get; private set; }

        public static string YoloGpuDllFilename1 => YoloGpuDllFilename;

        public static string YoloGpuDllFilename2 => YoloGpuDllFilename;

        public IEnumerable<YoloItem> Detect(string filepath)
		{
			if (!File.Exists(filepath))
				throw new FileNotFoundException("Cannot find the file", filepath);
			var container = new BboxContainer();
			DetectImageGpu(filepath, ref container);
			return Convert(container);
		}

		public IEnumerable<YoloItem> Detect(ColorData imageData,
			int channels = 3, bool track = false)
		{
			return track ? Track(ToYoloRgbFormat(imageData)) : Detect(ToYoloRgbFormat(imageData));
		}

		public IEnumerable<YoloItem> Detect(byte[] byteData, int width, int height,
			int channels = 3, bool track = false)
		{
			if (!imageAnalyzer.IsValidImageFormat(byteData))
				throw new Exception("Invalid image data, wrong image format");

			var imageData = BitmapToColorData((Bitmap) Image.FromStream(new MemoryStream(byteData)));
			return track
				? Track(ColorDataToYoloRgbFormat(imageData, channels), imageData.Width, imageData.Height,
					channels)
				: Detect(ColorDataToYoloRgbFormat(imageData, channels), imageData.Width, imageData.Height,
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

			return Convert(container);
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

			return Convert(container);
		}

		private IEnumerable<YoloItem> Convert(BboxContainer container)
		{
			return container.candidates.Where(o => o.h > 0 || o.w > 0)
				.Select(item => new YoloItem
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
		}

		public ColorData BitmapToColorData(Bitmap image)
		{
			var colorData = new ColorData
			{
				Width = image.Width,
				Height = image.Height,
				Colors = new Color[image.Width * image.Height]
			};

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
					colorData.Colors[x] = new Color(r, g, b, a);
				}

				image.UnlockBits(bmpData);
			}

			return colorData;
		}

		// ReSharper disable once TooManyDeclarations
		public unsafe IntPtr ColorDataToYoloRgbFormat(ColorData imageData, int channels)
		{
			var sizeInBytes = imageData.Width * imageData.Height * channels * sizeof(float);
			var floatArrayPointer = Marshal.AllocHGlobal(sizeInBytes);
			var destination = (float*) floatArrayPointer.ToPointer();
			// yolo needs the data in format like red all, green all, blue all.
			for (var channel = 0; channel < channels; channel++)
			for (var y = 0; y < imageData.Height; y++)
			for (var x = 0; x < imageData.Width; x++)
			{
				var color = imageData.Colors[x + y * imageData.Width];

				*destination++ = channel switch
				{
					0 => color.RedValue,
					1 => color.GreenValue,
					2 => color.BlueValue,
					_ => color.AlphaValue
				};
			}

			return floatArrayPointer;
		}

		public static unsafe IntPtr ToYoloRgbFormat(ColorData colorData, int yoloWidth = 416,
			int yoloHeight = 416, int channels = 3)
		{
			var sizeInBytes = yoloWidth * yoloHeight * channels * sizeof(float);
			var floatArrayPointer = Marshal.AllocHGlobal(sizeInBytes);
			var destination = (float*) floatArrayPointer.ToPointer();

			for (var channel = 0; channel < channels; channel++)
			for (var y = 0; y < yoloHeight; y++)
			for (var x = 0; x < yoloWidth; x++)
			{
				var imageX = x * colorData.Width / yoloWidth;
				var imageY = y * colorData.Height / yoloHeight;
				var color = colorData.Colors[imageX + imageY * colorData.Width];
				*destination++ = channel switch
				{
					0 => color.RedValue,
					1 => color.GreenValue,
					_ => color.BlueValue
				};
			}

			return floatArrayPointer;
		}

		public byte[] ToByteArray(Image image, ImageFormat format)
		{
			using var ms = new MemoryStream();
			image.Save(ms, format);
			return ms.ToArray();
		}
	}
}