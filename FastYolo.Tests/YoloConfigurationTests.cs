using NUnit.Framework;
using static FastYolo.ConfigurationDetector;

namespace FastYolo.Tests;

public sealed class YoloConfigurationTests
{
#if WIN64
	private const string YoloServerDirectory = @"Assets/";
#else
	private const string YoloServerDirectory = @"/home/abdul/Code/GitHub/FastYolo/FastYolo.Tests/Assets/";
#endif
	public const string ImageFilename = YoloServerDirectory + "cars road.jpg";
	public const string YoloWeightsFilename = YoloServerDirectory + @"yolov3-tiny.weights";
	public const string YoloConfigFilename = YoloServerDirectory + "yolov3-tiny.cfg";
	public const string YoloClassesFilename = YoloServerDirectory + "coco.names";

	[Test]
	public void ConfigurationFilesExists()
	{
		var yoloConfig = CheckConfiguration(YoloServerDirectory);
		Assert.That(Directory.Exists(YoloServerDirectory), Is.True);
		Assert.That(yoloConfig.ConfigFile, Is.EqualTo(YoloConfigFilename));
		Assert.That(yoloConfig.NamesFile, Is.EqualTo(YoloClassesFilename));
		Assert.That(yoloConfig.WeightsFile, Is.EqualTo(YoloWeightsFilename));
		Assert.That(File.Exists(ImageFilename), Is.True);
	}
}