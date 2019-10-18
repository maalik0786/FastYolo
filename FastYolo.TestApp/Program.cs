using System;
using FastYolo.Tests;

namespace FastYolo.TestApp
{
	public class Program
	{
		//ncrunch: no coverage start
		public static void Main()
		{
			var tests = new YoloTests();
			tests.Setup();
			tests.LoadDummyImageForObjectDetection();
			Console.WriteLine("LoadDummyImageForObjectDetection() Done\n");
			tests.ByteArrayForObjectDetection();
			Console.WriteLine("ByteArrayForObjectDetection() Done\n");
			tests.PassIntPtrForObjectTracking();
			Console.WriteLine("PassIntPtrForObjectTracking() Done\n");
			tests.LoadColorDataForObjectDetection();
			Console.WriteLine("LoadColorDataForObjectDetection() Done\n");
		} //ncrunch: no coverage end
	}
}