# FastYolo

Yolo Object Detection library for .NET 7. This one requires Cuda 11.7 and CudNN 8.4.1 to be installed to work, it will use your NVidia GPU for processing (which is many times faster than using the CPU version that is disabled in this release). Any GPU from Maxwell upwards is support (Cuda api 5.2+, Maxwell GPUs like GTX 750 and above). Since 11.1 it also supports OpenCV for more advanced features like tracking, shape detection, etc.
			
Includes the .dll files for Yolo Darknet Wrapper, Real-Time Object Detection (yolo core of AlexeyAB/darknet), including opencv_world460.dll and pthreadVC2.dll as needed by the yolo_cpp_dll implementation.

#### Different operating systems should have different cuda versions:
1. Windows: should have Cuda 11.7 and CUDNN 8.3.2+ installed, if not an exception is thrown with detailed installation instructions.
Environment path for cuda 11.7 must be set (installer does this), e.g. CUDA_PATH=C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v11.7
2. If you get error: Could not load library cudnn_cnn_infer64_8.dll. Error code 126, 
Please follow [this link](https://docs.nvidia.com/deeplearning/cudnn/install-guide/index.html#install-zlib-windows) for proper installation of CUDNN
3. Linux x64:  should have Cuda 11.7 and CUDNN 8.4.1+ installed, if not an exception is thrown with detailed installation instructions.
4. Jetson Nano Arm64:  should have Cuda 10.0 and CUDNN 8 installed, if not an exception is thrown with detailed installation instructions.
5. Jetson Xavier Arm64:  should have Cuda 10.2 and CUDNN 8 installed, if not an exception is thrown with detailed installation instructions.

The nuget installer includes all other needed files, for compiling it yourself, copy cudnn64_8.dll, opencv_world460.dll, pthreadVC2.dll into the FastYolo folder and compile yolo_cpp_dll.dll into it as well.

Current version is for .NET 7, you can check older releases for .NET6, .NET 5, .NET Core 3.1, .NET 4.6 and lower.

#### How to use:

1. Create C# Console project
2. add the Nuget Package as a reference in your project 
 ```ini 
 <ItemGroup>
   <PackageReference Include="FastYolo" Version="11.7.3" />
 </ItemGroup>
 ```
3. open Program.cs file and paste the code below
4. For complete examples please visit [here](https://github.com/maalik0786/FastYolo/blob/master/FastYolo.TestApp/Program.cs)

#### Required files to run the code below:

1. YoloConfigFile is available [here](https://github.com/maalik0786/FastYolo/blob/master/FastYolo.Tests/Assets/yolov3-tiny.cfg)
2. YoloWeightsFile is available [here](https://pjreddie.com/media/files/yolov3-tiny.weights)
3. YoloClassesFile is available [here](https://github.com/maalik0786/FastYolo/blob/master/FastYolo.Tests/Assets/coco.names)
4. ImageFilename is available [here](https://github.com/maalik0786/FastYolo/blob/master/FastYolo.Tests/Assets/cars%20road.jpg)

```ini
YoloWrapper yoloWrapper = new YoloWrapper(YoloConfigFile, YoloWeightsFile, YoloClassesFile);

var image = Image.FromFile(ImageFilename);
using var memoryStream = new MemoryStream();
image.Save(memoryStream, image.RawFormat);
var byteArray = memoryStream.ToArray();

IEnumerable<YoloItem> yoloItems yoloWrapper.Detect(byteArray);

foreach (var item in yoloItems)
  Console.WriteLine($"Object Found: {item.Name} with Shape: {item.Shape}, X: {item.X}, Y: {item.Y}, Width: {item.Width}, Height: {item.Height}"); 
```
For complete examples please visit [here](https://github.com/maalik0786/FastYolo/blob/master/FastYolo.Tests/YoloWrapperTests.cs)
