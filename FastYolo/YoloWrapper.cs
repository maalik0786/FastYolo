using System.Text;
using FastYolo.Model;
using System.Runtime.InteropServices;
using static FastYolo.ImageConverter;

namespace FastYolo;

// ReSharper disable once HollowTypeName
public sealed class YoloWrapper : IDisposable
{
	private readonly YoloObjectTypeResolver objectTypeResolver;
	// ReSharper disable once UnusedAutoPropertyAccessor.Local
	public string? GraphicDeviceName { get; set; }
	public const int MaxObjects = 30;

	public YoloWrapper(string configurationFilename, string weightsFilename,
		string namesFilename, int gpu = 0)
	{
		if (!File.Exists(configurationFilename))
			throw new FileNotFoundException(configurationFilename);
		if (!File.Exists(weightsFilename))
			throw new FileNotFoundException(weightsFilename);
		if (!File.Exists(namesFilename))
			throw new FileNotFoundException(namesFilename);
		objectTypeResolver = new YoloObjectTypeResolver(namesFilename);
		Initialize(configurationFilename, weightsFilename, gpu);
	}

#if WIN64
	private const string YoloGpuDllFilename = "yolo_cpp_dll.dll";
	private const string YoloPThreadDllFilename = "pthreadVC2.dll";
	private const string CudnnDllFilename = "cudnn64_8.dll";
	private const string CudnnRequiredDependencyFilename = "cudnn_ops_infer64_8.dll";
	private const string CudaVersion = "11.7";
	private const string CudnnVersion = "8.4.1";
#if DEBUG
	private const string OpenCvWorldDllFilename = "opencv_world460d.dll";
#else
	private const string OpenCvWorldDllFilename = "opencv_world460.dll";
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

	[DllImport(YoloGpuDllFilename, EntryPoint = "detect_mat")]
	private static extern int
		DetectImage(IntPtr pArray, int nSize, ref BboxContainer container);

	[DllImport(YoloGpuDllFilename, EntryPoint = "detect_objects")]
	// ReSharper disable TooManyArguments
	private static extern int DetectObjectsGpu(IntPtr pArray, int width, int height,
		int channel, ref BboxContainer container);

	[DllImport(YoloGpuDllFilename, EntryPoint = "detect_objects_cuda")]
	private static extern int DetectObjectsCuda(IntPtr pArray, int width, int height,
		int channel, ref BboxContainer container);

	[DllImport(YoloGpuDllFilename, EntryPoint = "track_objects")]
	private static extern int TrackObjectsGpu(IntPtr pArray, int width, int height, int channel,
		ref BboxContainer container);

	/// <summary>
	/// Resizing images every frame is VERY slow and should be avoided, use the given size from the
	/// configuration file passed into the constructor here (usually 416x416), resizing should not be
	/// done dynamically in the darknet library, extra memory would be required and it is obviously
	/// slower than if we already have 416x416 data!
	/// </summary>
	[DllImport(YoloGpuDllFilename, EntryPoint = "get_detector_network_width")]
	public static extern int GetDetectorNetworkWidth();

	[DllImport(YoloGpuDllFilename, EntryPoint = "get_detector_network_height")]
	public static extern int GetDetectorNetworkHeight();

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
#if WIN64
		const string CudaError = "An Nvidia GPU and CUDA " + CudaVersion +
			" need to be installed! Please install CUDA " +
			"https://developer.nvidia.com/cuda-downloads\nError details: ";
		if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUDA_PATH")))
			throw new DllNotFoundException(CudaError +
				"CUDA_PATH environment variable is not available!");
		var cudaBinPath = Path.Combine(Environment.GetEnvironmentVariable("CUDA_PATH")!, "bin");
		if (!File.Exists(Path.Combine(cudaBinPath, "CUBLAS64_11.DLL")))
			throw new DllNotFoundException(CudaError +
				@"cublas64_11.dll wasn't found in the CUDA_PATH\bin folder " +
				"(did you maybe install CUDA 10.* and not CUDA " + CudaVersion + "+, " +
				"please install it again or fix your CUDA_PATH)");
		if (!File.Exists(Path.Combine(cudaBinPath, "CUDART64_110.DLL")))
			throw new DllNotFoundException(CudaError +
				@"cudart64_110.dll wasn't found in the CUDA_PATH\bin folder " +
				"(did you maybe install CUDA 10.* and not CUDA " + CudaVersion + "+, " +
				"please install it again or fix your CUDA_PATH)");
		if (!File.Exists(Path.Combine(cudaBinPath, "CURAND64_10.DLL")))
			throw new DllNotFoundException(CudaError +
				@"curand64_10.dll wasn't found in the CUDA_PATH\bin folder " +
				"(did you maybe install CUDA 10.* and not CUDA " + CudaVersion + "+, " +
				"please install it again or fix your CUDA_PATH)");
		if (!File.Exists(Path.Combine(Environment.SystemDirectory, "NVCUDA.DLL")))
			throw new DllNotFoundException(CudaError +
				"NVCUDA.DLL wasn't found in the windows system directory, " +
				"is CUDA and your Nvidia graphics driver correctly installed?");
		if (!File.Exists(Path.Combine(cudaBinPath, CudnnRequiredDependencyFilename)))
			throw new DllNotFoundException(CudaError +
				"Cudnn dependencies not in CUDA_PATH and CUDNN environment variable is not available, make " +
				"sure Cudnn " + CudnnVersion + " for Cuda " + CudaVersion +
				" is installed as well: https://developer.nvidia.com/rdp/cudnn-download");
		if (!File.Exists(Path.Combine(cudaBinPath, CudnnDllFilename)))
			throw new FileNotFoundException("Can't find the " + CudnnDllFilename);
		var path = Environment.GetEnvironmentVariable("PATH");
		if (string.IsNullOrEmpty(path) || !path.Contains(cudaBinPath))
			throw new DllNotFoundException(CudaError +
				"PATH does not contain CUDA bin folder like this: " + cudaBinPath);
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

	public IEnumerable<YoloItem> Detect(string filepath)
	{
		if (!File.Exists(filepath))
			throw new FileNotFoundException("Cannot find the file", filepath);
		var container = new BboxContainer();
		DetectImageGpu(filepath, ref container);
		return Convert(container, objectTypeResolver);
	}

	/// <summary>
	/// internally convert byte array into opencv:mat format
	/// </summary>
	/// <param name="imageData"></param>
	public IEnumerable<YoloItem> Detect(byte[] imageData)
	{
		var size = Marshal.SizeOf(imageData[0]) * imageData.Length;
		var pnt = Marshal.AllocHGlobal(size);
		var container = new BboxContainer();
		try
		{
			// Copy array to unmanaged memory.
			Marshal.Copy(imageData, 0, pnt, imageData.Length);
			var count = DetectImage(pnt, imageData.Length, ref container);
			if (count == -1)
				throw new NotSupportedException($"{YoloGpuDllFilename} has no OpenCV support");
		}
		catch (Exception)
		{
			return null!;
		}
		finally
		{
			// Free the unmanaged memory.
			Marshal.FreeHGlobal(pnt);
		}
		return Convert(container, objectTypeResolver);
	}

	/// <summary>
	/// Pass only Cpu memory pointer of Image (ColorImage)
	/// </summary>
	public IEnumerable<YoloItem> Detect(IntPtr floatArrayPointer, int width, int height,
		int channels = 3)
	{
		var container = new BboxContainer();
		DetectObjectsGpu(floatArrayPointer, width, height, channels, ref container);
		return Convert(container, objectTypeResolver);
	}

	/// <summary>
	/// Pass only Gpu memory pointer of Image (ColorImage)
	/// </summary>
	/// <param name="sizeTPointer">CudaDevice varaible of type float as a pointer</param>
	/// <param name="width">Width of the Image</param>
	/// <param name="height">Height of the Image</param>
	/// <param name="channels">Image Channels like RGB => 3</param>
	public IEnumerable<YoloItem> DetectCuda(IntPtr sizeTPointer, int width, int height,
		int channels = 3)
	{
		var container = new BboxContainer();
		try
		{
			DetectObjectsCuda(sizeTPointer, width, height, channels, ref container);
		}
		catch (Exception)
		{
			var networkWidth = GetDetectorNetworkWidth();
			var networkHeight = GetDetectorNetworkHeight();
			if (networkWidth != width && networkHeight != height)
				throw new ImageSizeIsNotSameAsYoloConfiguration(
					"Input image size: " + width + "x" + height +
					" is not same size as in configuration file: " + networkWidth + "x" +
					networkHeight);
			throw new InvalidDataException();
		}
		return Convert(container, objectTypeResolver);
	}

	/// <summary>
	/// Pass only Cpu memory pointer of Image (ColorImage)
	/// </summary>
	public IEnumerable<YoloItem> Track(IntPtr floatArrayPointer, int width, int height,
		int channel = 3)
	{
		var container = new BboxContainer();
		TrackObjectsGpu(floatArrayPointer, width, height, channel, ref container);
		return Convert(container, objectTypeResolver);
	}

	public void Dispose() => DisposeYoloGpu();
}

internal sealed class ImageSizeIsNotSameAsYoloConfiguration : Exception
{
	public ImageSizeIsNotSameAsYoloConfiguration(string info) : base(info) { }
}