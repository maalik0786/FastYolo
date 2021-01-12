# FastYolo

Yolo library for .NET 5. This one requires Cuda 11.1 and CudNN 8 to be installed to work, it will use the GPU for processing (which is many times faster than using the CPU version that is disabled in this release). Any GPU from Maxwell upwards is support (Cuda api 5.2+, Maxwell GPUs like GTX 750 and above). Since 11.1 it also supports OpenCV for more advanced features like tracking, shape detection, etc.
			
Includes the .dll files for Yolo Darknet Wrapper, Real-Time Object Detection (yolo core of AlexeyAB/darknet), including opencv_world440.dll and pthreadVC2.dll as needed by the yolo_cpp_dll implementation.

User should have Cuda 11.1+ and CUDNN 8.0.5+ installed, if not an exception is thrown with detailed installation instructions.
Environment path for cuda 11.1 must be set (installer does this), e.g. CUDA_PATH=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v11.1
The nuget installer includes all other needed files, for compiling it yourself, copy cudnn64_7.dll, opencv_world440.dll, pthreadVC2.dll into the FastYolo folder and compile yolo_cpp_dll.dll into it as well.

Current version is for .NET 5, you can check older releases for .NET Core 3.1, .NET 4.6 and lower.