using System.Collections.Generic;
using FastYolo.Model;
using NUnit.Framework;
using static FastYolo.DrawSquare;

namespace FastYolo.Tests
{
	public sealed class DrawSquareTests
	{
		private static ColorImage CreateTestColorData() =>
			new(new Size(10, 10))
			{
				Colors = new[]
				{
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black,
					Color.Black, Color.Black, Color.Black, Color.Black
				}
			};

		[Test]
		public void DrawObjectFrameTest()
		{
			var colorData = CreateTestColorData();
			var simpleDrawings = new DrawSquare();
			simpleDrawings.SetColor(Color.Red);
			DrawObjectFrame(colorData, 5, 5, 3, 3);
			Assert.That(colorData.Colors[55].R, Is.EqualTo(255));
		}

		[Test]
		public void DrawObjectFrameTest2()
		{
			var colorData = CreateTestColorData();
			DrawObjectFrame(colorData, 0, 0, 1, 1);
			Assert.That(colorData.Colors[55].R, Is.EqualTo(0));
		}

		[Test]
		public void DrawBoundingBoxTest()
		{
			var items = new List<YoloItem>
			{
				new()
				{
					Width = 4,
					Height = 3,
					X = 7,
					Y = 2,
					Type = "Apple"
				},
				new()
				{
					Width = 3,
					Height = 4,
					X = 5,
					Y = 5,
					Type = "Apple"
				}
			};
			DrawBoundingBox(CreateTestColorData(), items);
			Assert.That(items, Is.Not.Null);
		}
	}
}