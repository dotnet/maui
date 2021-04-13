using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class LinearGradientBrushTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
		}

		[Test]
		public void TestConstructor()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

			Assert.AreEqual(1.0d, linearGradientBrush.EndPoint.X, "EndPoint.X");
			Assert.AreEqual(1.0d, linearGradientBrush.EndPoint.Y, "EndPoint.Y");
		}

		[Test]
		public void TestConstructorUsingGradientStopCollection()
		{
			var gradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.Red, Offset = 0.1f },
				new GradientStop { Color = Colors.Orange, Offset = 0.8f }
			};

			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientStops, new Point(0, 0), new Point(0, 1));

			Assert.AreNotEqual(0, linearGradientBrush.GradientStops.Count, "GradientStops");
			Assert.AreEqual(0.0d, linearGradientBrush.EndPoint.X, "EndPoint.X");
			Assert.AreEqual(1.0d, linearGradientBrush.EndPoint.Y, "EndPoint.Y");
		}

		[Test]
		public void TestEmptyLinearGradientBrush()
		{
			LinearGradientBrush nullLinearGradientBrush = new LinearGradientBrush();
			Assert.AreEqual(true, nullLinearGradientBrush.IsEmpty, "IsEmpty");

			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Orange, Offset = 0.1f },
					new GradientStop { Color = Colors.Red, Offset = 0.8f }
				}
			};

			Assert.AreEqual(false, linearGradientBrush.IsEmpty, "IsEmpty");
		}

		[Test]
		public void TestNullOrEmptyLinearGradientBrush()
		{
			LinearGradientBrush nullLinearGradientBrush = null;
			Assert.AreEqual(true, Brush.IsNullOrEmpty(nullLinearGradientBrush), "IsNullOrEmpty");

			LinearGradientBrush emptyLinearGradientBrush = new LinearGradientBrush();
			Assert.AreEqual(true, Brush.IsNullOrEmpty(emptyLinearGradientBrush), "IsNullOrEmpty");

			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Orange, Offset = 0.1f },
					new GradientStop { Color = Colors.Red, Offset = 0.8f }
				}
			};

			Assert.AreEqual(false, Brush.IsNullOrEmpty(linearGradientBrush), "IsNullOrEmpty");
		}

		[Test]
		public void TestLinearGradientBrushPoints()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0)
			};

			Assert.AreEqual(0, linearGradientBrush.StartPoint.X);
			Assert.AreEqual(0, linearGradientBrush.StartPoint.Y);

			Assert.AreEqual(1, linearGradientBrush.EndPoint.X);
			Assert.AreEqual(0, linearGradientBrush.EndPoint.Y);
		}

		[Test]
		public void TestLinearGradientBrushOnlyOneGradientStop()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, }
				},
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0)
			};

			Assert.IsNotNull(linearGradientBrush);
		}

		[Test]
		public void TestLinearGradientBrushGradientStops()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, Offset = 0.1f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				},
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0)
			};

			Assert.AreEqual(2, linearGradientBrush.GradientStops.Count);
		}
	}
}