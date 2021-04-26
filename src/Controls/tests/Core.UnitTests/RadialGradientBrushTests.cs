using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class RadialGradientBrushTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
		}

		[Test]
		public void TestConstructor()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush();

			int gradientStops = radialGradientBrush.GradientStops.Count;

			Assert.AreEqual(0, gradientStops);
		}

		[Test]
		public void TestConstructorUsingGradientStopCollection()
		{
			var gradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.Red, Offset = 0.1f },
				new GradientStop { Color = Colors.Orange, Offset = 0.8f }
			};

			RadialGradientBrush radialGradientBrush = new RadialGradientBrush(gradientStops, new Point(0, 0), 10);

			Assert.AreNotEqual(0, radialGradientBrush.GradientStops.Count, "GradientStops");
			Assert.AreEqual(0, radialGradientBrush.Center.X, "Center.X");
			Assert.AreEqual(0, radialGradientBrush.Center.Y, "Center.Y");
			Assert.AreEqual(10, radialGradientBrush.Radius, "Radius");
		}

		[Test]
		public void TestEmptyRadialGradientBrush()
		{
			RadialGradientBrush nullRadialGradientBrush = new RadialGradientBrush();
			Assert.AreEqual(true, nullRadialGradientBrush.IsEmpty, "IsEmpty");

			RadialGradientBrush radialGradientBrush = new RadialGradientBrush
			{
				Center = new Point(0, 0),
				Radius = 10,
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Orange, Offset = 0.1f },
					new GradientStop { Color = Colors.Red, Offset = 0.8f }
				}
			};

			Assert.AreEqual(false, radialGradientBrush.IsEmpty, "IsEmpty");
		}

		[Test]
		public void TestNullOrEmptyRadialGradientBrush()
		{
			RadialGradientBrush nullRadialGradientBrush = null;
			Assert.AreEqual(true, Brush.IsNullOrEmpty(nullRadialGradientBrush), "IsNullOrEmpty");

			RadialGradientBrush emptyRadialGradientBrush = new RadialGradientBrush();
			Assert.AreEqual(true, Brush.IsNullOrEmpty(emptyRadialGradientBrush), "IsNullOrEmpty");

			RadialGradientBrush radialGradientBrush = new RadialGradientBrush
			{
				Center = new Point(0, 0),
				Radius = 10,
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Orange, Offset = 0.1f },
					new GradientStop { Color = Colors.Red, Offset = 0.8f }
				}
			};

			Assert.AreEqual(false, Brush.IsNullOrEmpty(radialGradientBrush), "IsNullOrEmpty");
		}

		[Test]
		public void TestRadialGradientBrushRadius()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush();
			radialGradientBrush.Radius = 20;

			Assert.AreEqual(20, radialGradientBrush.Radius);
		}

		[Test]
		public void TestRadialGradientBrushOnlyOneGradientStop()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, }
				},
				Radius = 20
			};

			Assert.IsNotNull(radialGradientBrush);
		}

		[Test]
		public void TestRadialGradientBrushGradientStops()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, Offset = 0.1f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				},
				Radius = 20
			};

			Assert.AreEqual(2, radialGradientBrush.GradientStops.Count);
		}
	}
}