namespace FastYolo
{
	public class YoloConfiguration
	{
		public YoloConfiguration(string configFile, string weightsFile, string namesFile)
		{
			ConfigFile = configFile;
			WeightsFile = weightsFile;
			NamesFile = namesFile;
		}

		public string ConfigFile { get; set; }
		public string WeightsFile { get; set; }
		public string NamesFile { get; set; }
	}
}