# FastYolo

Improved version of Alturos Yolo library. This one requires Cuda 10.1 to be installed to work, it will use the GPU for processing (which is many times faster than using the CPU version that is disabled in this release). Any GPU from Maxwell upwards is support (Cuda api 5.2+, Maxwell GPUs like GTX 750 and above). Since 1.0.5 it also supports OpenCV for more advanced features like tracking, shape detection, etc.
			
Includes the .dll files for Yolo Darknet Wrapper, Real-Time Object Detection (yolo core of AlexeyAB/darknet), including opencv_world420.dll and pthreadVC2.dll as needed by the yolo_cpp_dll implementation.

This library requires CudNN 7.6+ for Cuda 10.1 to be installed to work, it will use the GPU for processing!

User should have Cuda 10.1 and CUDNN 7.6.x.xx installed, if not an exception is thrown with detailed installation instructions.
Environment path for cuda 10.1 must be set (installer does this), e.g. CUDA_PATH=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.1
User has to copy cudnn64_7.dll into the CUDA bin folder (CUDA_PATH/bin)
CUDART64_101.DLL, CURAND64_10.DLL, CUBLAS64_10.DLL should be in CUDA_PATH/bin (C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.1\bin)
PATH should contain CUDA_PATH/bin
Finally we will also check if NVCUDA.DLL is installed on %SYSTEM32% (C:\windows\system32).

About nuget, its not easy to get the native files copied, we are using this solution now, it must be done via .target file and both managed and native dlls must be included correctly:
https://stackoverflow.com/questions/10198428/where-to-place-dlls-for-unmanaged-libraries

also useful tips, but didn't work well for us:
https://stackoverflow.com/questions/19478775/add-native-files-from-nuget-package-to-project-output-directory/30316946

also has net framework 4.6.1+ compatibility now via: <group targetFramework=".NETFramework4.6.1">...