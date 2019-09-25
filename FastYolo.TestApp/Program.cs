using FastYolo.Tests;

namespace FastYolo.TestApp
{
	public class Program
	{
		public static void Main()
		{
			var tests = new YoloTests();
			tests.Setup();
			tests.LoadJpegFromRaspberryCamera();
		}
	}
}