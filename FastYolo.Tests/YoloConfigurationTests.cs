using System.IO;
using NUnit.Framework;
using static FastYolo.ConfigurationDetector;

namespace FastYolo.Tests
{
	public class YoloConfigurationTests
	{
#if WIN64
		public const string YoloServerDirectory = @"\\DeltaServer\Builds\yolo-v3-tiny\";
#else
		public const string YoloServerDirectory = "/home/abdul/Documents/yolo-v3-tiny/CurrentTraining/";
#endif
		public const string DummyImageFilename = YoloServerDirectory + "DummyNutInput.png";
		public const string DummyImageOutputFilename = YoloServerDirectory + "DummyNutOutput.png";
		public const string YoloWeightsFilename = YoloServerDirectory + "yolov3-tiny-prn-train_last.weights";
		public const string YoloConfigFilename = YoloServerDirectory + "yolov3-tiny-prn-test.cfg";
		public const string YoloClassesFilename = YoloServerDirectory + "classes.names";

		[Test]
		public void ConfigurationFilesExists()
		{
			var yoloConfig = CheckConfiguration(YoloServerDirectory);
			Assert.That(Directory.Exists(YoloServerDirectory), Is.True);
			Assert.That(yoloConfig.ConfigFile, Is.EqualTo(YoloConfigFilename));
			Assert.That(yoloConfig.NamesFile, Is.EqualTo(YoloClassesFilename));
			Assert.That(yoloConfig.WeightsFile, Is.EqualTo(YoloWeightsFilename));
			Assert.That(File.Exists(DummyImageFilename), Is.True);
			Assert.That(File.Exists(DummyImageOutputFilename), Is.True);
		}
	}
}
