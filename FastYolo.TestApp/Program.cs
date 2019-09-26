using System;
using System.Threading.Tasks.Sources;
using System.Security.Cryptography;
using System.Globalization;
using FastYolo.Tests;

namespace FastYolo.TestApp
{
	public class Program
	{
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
		}
	}
}