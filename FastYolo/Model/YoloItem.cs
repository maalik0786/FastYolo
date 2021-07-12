namespace FastYolo.Model
{
	// ReSharper disable once HollowTypeName
	public sealed class YoloItem
	{
		public enum ShapeType
		{
			None,
			Triangle,
			Rectangle,
			Penta,
			Hexa,
			HalfCircle,
			Circle
		}

		public string? Type { get; set; }
		public double Confidence { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int TrackId { get; set; }
		public int FrameId { get; set; }
		public ShapeType Shape { get; set; }
	}
}