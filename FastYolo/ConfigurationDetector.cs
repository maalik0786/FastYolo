using System.IO;
using System.Linq;

namespace FastYolo
{
	public static class ConfigurationDetector
	{
		/// <summary>
		///   Automatict detect the yolo configuration on the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException">Thrown when cannot found one of the required yolo files</exception>
		public static YoloConfiguration CheckConfiguration(string path = ".")
		{
			var files = GetYoloFiles(path);
			var yoloConfiguration = MapFiles(files);
			var configValid = AreValidYoloFiles(yoloConfiguration);

			if (configValid) return yoloConfiguration;
			//ncrunch: no coverage start
			throw new FileNotFoundException(
				"Cannot found pre-trained model, check all config files available (.cfg, .weights, .names)"); //ncrunch: no coverage end
		}

		private static string[] GetYoloFiles(string path)
		{
			return Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(o =>
				o.EndsWith(".names") || o.EndsWith(".cfg") || o.EndsWith(".weights")).ToArray();
		}

		private static YoloConfiguration MapFiles(string[] files)
		{
			var configurationFile = files.FirstOrDefault(o => o.EndsWith(".cfg"));
			var weightsFile = files.FirstOrDefault(o => o.EndsWith(".weights"));
			var namesFile = files.FirstOrDefault(o => o.EndsWith(".names"));

			return new YoloConfiguration(configurationFile, weightsFile, namesFile);
		}

		private static bool AreValidYoloFiles(YoloConfiguration config) =>

			!string.IsNullOrEmpty(config.ConfigFile) && !string.IsNullOrEmpty(config.WeightsFile) && !string.IsNullOrEmpty(config.NamesFile);
	}
}