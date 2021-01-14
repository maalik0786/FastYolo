using System;
using FastYolo.Tests;

namespace FastYolo.TestApp
{
	public class Program
	{
		//ncrunch: no coverage start
		public static void Main()
		{
			using var yoloWrapper = new YoloWrapper(YoloConfigurationTests.YoloConfigFilename, YoloConfigurationTests.YoloWeightsFilename, YoloConfigurationTests.YoloClassesFilename);
				var yoloItems = yoloWrapper.Detect(YoloConfigurationTests.DummyImageFilename);
				foreach (var item in yoloItems)
					Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
				Console.WriteLine("LoadDummyImageForObjectDetection() Done\n");
		}
	}
}