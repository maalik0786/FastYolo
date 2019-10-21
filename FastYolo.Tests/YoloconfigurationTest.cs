using System.IO;
using NUnit.Framework;

namespace FastYolo.Tests
{
	public class YoloConfigurationTest
	{
#if WIN64
		private const string YoloServerDirectory = @"\\DeltaServer\Shared\yolo-v3-tiny\";
#else
		private const string YoloServerDirectory = "/home/dev/Documents/yolo-v3-tiny/";
#endif
		private const string DummyImageFilename = YoloServerDirectory + "DummyNutInput.png";
		private const string DummyImageOutputFilename = YoloServerDirectory + "DummyNutOutput.jpg";
		private const string YoloWeightsFilename = YoloServerDirectory + "yolov3-tiny_walnut.weights";
		private const string YoloConfigFilename = YoloServerDirectory + "yolov3-tiny_walnut.cfg";
		private const string YoloClassesFilename = YoloServerDirectory + "classes.names";

		[Test]
		public void ConfigurationFilesExists()
		{
			var yoloConfig = new ConfigurationDetector().CheckConfiguration(YoloServerDirectory);
			Assert.That(Directory.Exists(YoloServerDirectory), Is.True);
			Assert.That(yoloConfig.ConfigFile, Is.EqualTo(YoloConfigFilename));
			Assert.That(yoloConfig.NamesFile, Is.EqualTo(YoloClassesFilename));
			Assert.That(yoloConfig.WeightsFile, Is.EqualTo(YoloWeightsFilename));
			Assert.That(File.Exists(DummyImageFilename), Is.True);
			Assert.That(File.Exists(DummyImageOutputFilename), Is.True);
		}
	}
}
