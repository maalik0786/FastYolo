using System;
using System.IO;
using System.Linq;

namespace FastYolo
{
	public static class ConfigurationDetector
	{
		public static YoloConfiguration CheckConfiguration(string path = ".")
		{
			var files = GetYoloFiles(path);
			var yoloConfiguration = MapFiles(files);
			var configValid = AreValidYoloFiles(yoloConfiguration);
			if (configValid)
				return yoloConfiguration;
			//ncrunch: no coverage start
			throw new FileNotFoundException(
				"Cannot found pre-trained model, check all config files available (.cfg, .weights, .names)"); //ncrunch: no coverage end
		}

		private static string[] GetYoloFiles(string path) =>
			Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(o =>
				o.EndsWith(".names", StringComparison.Ordinal) || o.EndsWith(".cfg", StringComparison.Ordinal) || o.EndsWith(".weights", StringComparison.Ordinal)).ToArray();

		// ReSharper disable once TooManyDeclarations
		private static YoloConfiguration MapFiles(string[] files)
		{
			var configurationFile = files.FirstOrDefault(o => o.EndsWith(".cfg", StringComparison.Ordinal));
			var weightsFile = files.FirstOrDefault(o => o.EndsWith(".weights", StringComparison.Ordinal));
			var namesFile = files.FirstOrDefault(o => o.EndsWith(".names", StringComparison.Ordinal));
			return new YoloConfiguration(configurationFile!, weightsFile!, namesFile!);
		}

		private static bool AreValidYoloFiles(YoloConfiguration config) =>
			!string.IsNullOrEmpty(config.ConfigFile) &&
			!string.IsNullOrEmpty(config.WeightsFile) && !string.IsNullOrEmpty(config.NamesFile);
	}
}