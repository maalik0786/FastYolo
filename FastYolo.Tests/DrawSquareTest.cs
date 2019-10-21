using System.Linq;
using FastYolo.Model;
using NUnit.Framework;

namespace FastYolo.Tests
{
	public class DrawSquareTest
	{
		private static ColorData CreateTestColorData()
		=> new ColorData
			{
				Height = 10,
				Width = 10,
				Colors = new[]
				{
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black
				}
			};

		[Test]
		public void DrawObjectFrameTest()
		{
			var colorData = CreateTestColorData();
			var simpleDrawings = new DrawSquare();
			simpleDrawings.SetColor(Color.Red);
			simpleDrawings.DrawObjectFrame(colorData, 5,5,3,3);
			Assert.That(colorData.Colors[55].R, Is.EqualTo(255));
		}

		[Test]
		public void DrawObjectFrameTest2()
		{
			var colorData = CreateTestColorData();
			var simpleDrawings = new DrawSquare();
			simpleDrawings.DrawObjectFrame(colorData, 0,0,1,1);
			Assert.That(colorData.Colors[55].R, Is.EqualTo(0));
		}

		[Test]
		public void DrawBoundingBoxTest()
		{
			var items = Enumerable.Empty<YoloItem>().ToList();
			items.Append(new YoloItem { Width = 4, Height = 3, X = 7, Y = 2, Type = "Walnut"});
			items.Append(new YoloItem { Width = 3, Height = 4, X = 5, Y = 5, Type = "Walnut"});
			Assert.That(items, Is.Not.Null);
			new DrawSquare().DrawBoundingBox(CreateTestColorData(), items);
		}
	}
}
