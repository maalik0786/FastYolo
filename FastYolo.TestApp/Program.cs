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
			tests.PassIntPtrForObjectTracking();
			Console.WriteLine("PassIntPtrForObjectTracking() Done\n");
			tests.DisposeYoloWrapper();
		}
	}
}