using System;
using FastYolo.Tests;

namespace FastYolo.TestApp
{
	public class Program
	{
		//ncrunch: no coverage start
		public static void Main()
		{
			var yoloWrapper = new YoloWrapper(YoloConfigurationTests.YoloConfigFilename, YoloConfigurationTests.YoloWeightsFilename, YoloConfigurationTests.YoloClassesFilename);
			var floatArray = new FloatArray();
			//string path = "/home/abdul/code/";
			//string fileName = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + "_Logs.txt";
			try
			{
				 var yoloItems = yoloWrapper.Detect(YoloConfigurationTests.DummyImageFilename);
				if (yoloItems == null)
					{
						Console.WriteLine("No object Detected!");
						return;
					}
				foreach (var item in yoloItems)
					Console.WriteLine("Found " + item.Type + " " + item.X + "," + item.Y);
        //using StreamWriter file = new StreamWriter(path + fileName);
				//var tests = new YoloTests();
				//tests.Setup();
				//tests.LoadDummyImageForObjectDetection();
				Console.WriteLine("LoadDummyImageForObjectDetection() Done\n");
				//tests.ByteArrayForObjectDetection();
				//Console.WriteLine("ByteArrayForObjectDetection() Done\n");
				//tests.PassIntPtrForObjectTracking();
				//System.Console.WriteLine("PassIntPtrForObjectTracking() Done\n");
				//tests.LoadColorDataForObjectDetection();
				//Console.WriteLine("LoadColorDataForObjectDetection() Done\n");
			}
			catch (Exception) { }
		} //ncrunch: no coverage end
	}
}