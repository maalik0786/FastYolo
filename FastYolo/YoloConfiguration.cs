namespace FastYolo
{
	public sealed class YoloConfiguration
	{
		public YoloConfiguration(string configFile, string weightsFile, string namesFile)
		{
			ConfigFile = configFile;
			WeightsFile = weightsFile;
			NamesFile = namesFile;
		}

		public string ConfigFile { get; }
		public string WeightsFile { get; }
		public string NamesFile { get; }
	}
}