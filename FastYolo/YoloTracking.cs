using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FastYolo.Model;

namespace FastYolo
{
	public class YoloTracking
	{
		private readonly int _maxDistance;
		private readonly YoloWrapper _yoloWrapper;

		private int _index;
		private Point _trackingObject;

		public YoloTracking(YoloWrapper yoloWrapper, int maxDistance = 1000)
		{
			_yoloWrapper = yoloWrapper;
			_maxDistance = maxDistance;
		}

		public void SetTrackingObject(YoloItem trackingObject)
		{
			_trackingObject = trackingObject.Center();
		}

		public void SetTrackingObject(Point trackingObject)
		{
			_trackingObject = trackingObject;
		}

		public IEnumerable<YoloItem> Analyse(ColorData colorData, bool track = true)
		{
			var yoloItems = _yoloWrapper.Detect(colorData);
			var trackingObjects = new List<YoloItem>();
			//var enumerable = yoloItems as YoloItem[] ?? yoloItems.ToArray();
			foreach (var item in yoloItems)
			{
				var probableObject = FindBestMatch(yoloItems, _maxDistance);
				if (item == null) return null;

				_trackingObject = item.Center();
				trackingObjects.Add(new YoloTrackingItem(item, _index++));
				//var taggedImageData = DrawImage(imageData, probableObject);
			}

			return trackingObjects;
		}

		private YoloItem FindBestMatch(IEnumerable<YoloItem> items, int maxDistance)
		{
			var distanceItems = items
				.Select(o => new {Distance = Distance(o.Center(), _trackingObject), Item = o})
				.Where(o => o.Distance <= maxDistance).OrderBy(o => o.Distance);

			var bestMatch = distanceItems.FirstOrDefault();
			return bestMatch?.Item;
		}

		private static double Distance(Point p1, Point p2)
		{
			return Math.Sqrt(Pow2(p2.X - p1.X) + Pow2(p2.Y - p1.Y));
		}

		private static double Pow2(double x)
		{
			return x * x;
		}

		private static byte[] DrawImage(byte[] imageData, YoloItem item)
		{
			using (var memoryStream = new MemoryStream(imageData))
			using (var image = Image.FromStream(memoryStream))
			using (var canvas = Graphics.FromImage(image))
			using (var pen = new Pen(Brushes.Pink, 3))
			{
				canvas.DrawRectangle(pen, item.X, item.Y, item.Width, item.Height);
				canvas.Flush();
				using (var memoryStream2 = new MemoryStream())
				{
					image.Save(memoryStream2, ImageFormat.Bmp);
					return memoryStream2.ToArray();
				}
			}
		}
	}
}