namespace FastYolo.Model
{
	public class YoloTrackingItem : YoloItem
	{
		public YoloTrackingItem(YoloItem yoloItem, int index)
		{
			X = yoloItem.X;
			Y = yoloItem.Y;
			Width = yoloItem.Width;
			Height = yoloItem.Height;
			Type = yoloItem.Type;
			Confidence = yoloItem.Confidence;

			Index = index;
			//this.TaggedImageData = imageData;
		}

		public YoloTrackingItem(YoloItem yoloItem)
		{
			X = yoloItem.X;
			Y = yoloItem.Y;
			Width = yoloItem.Width;
			Height = yoloItem.Height;
			Type = yoloItem.Type;
			Confidence = yoloItem.Confidence;

			Index = yoloItem.TrackId;
			//this.TaggedImageData = imageData;
		}

		public int Index { get; set; }
		//public byte[] TaggedImageData { get; set; }
	}
}