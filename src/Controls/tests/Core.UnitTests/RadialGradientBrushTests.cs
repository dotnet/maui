using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class RadialGradientBrushTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush();

			int gradientStops = radialGradientBrush.GradientStops.Count;

			Assert.Equal(0, gradientStops);
		}

		[Fact]
		public void TestNullOrEmptyRadialGradientPaintWithEmptyGradientStop()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush
			{
				Center = new Point(0, 0),
				Radius = 10,
				GradientStops = new GradientStopCollection
				{
					new GradientStop(),
					new GradientStop()
				}
			};

			Paint radialGradientPaint = radialGradientBrush;

			Assert.True(radialGradientPaint.IsNullOrEmpty());
		}

		[Fact]
		public void TestNullOrEmptyRadialGradientPaintWithNullGradientStop()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush
			{
				Center = new Point(0, 0),
				Radius = 10,
				GradientStops = new GradientStopCollection
				{
					null,
					null
				}
			};

			Paint radialGradientPaint = radialGradientBrush;

			Assert.True(radialGradientPaint.IsNullOrEmpty());
		}

		[Fact]
		public void TestConstructorUsingGradientStopCollection()
		{
			var gradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.Red, Offset = 0.1f },
				new GradientStop { Color = Colors.Orange, Offset = 0.8f }
			};

			RadialGradientBrush radialGradientBrush = new RadialGradientBrush(gradientStops, new Point(0, 0), 10);

			Assert.NotEmpty(radialGradientBrush.GradientStops);
			Assert.Equal(0, radialGradientBrush.Center.X);
			Assert.Equal(0, radialGradientBrush.Center.Y);
			Assert.Equal(10, radialGradientBrush.Radius);
		}

		[Fact]
		public void TestEmptyRadialGradientBrush()
		{
			RadialGradientBrush nullRadialGradientBrush = new RadialGradientBrush();
			Assert.True(nullRadialGradientBrush.IsEmpty);

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

			Assert.False(radialGradientBrush.IsEmpty);
		}

		[Fact]
		public void TestNullOrEmptyRadialGradientBrush()
		{
			RadialGradientBrush nullRadialGradientBrush = null;
			Assert.True(Brush.IsNullOrEmpty(nullRadialGradientBrush));

			RadialGradientBrush emptyRadialGradientBrush = new RadialGradientBrush();
			Assert.True(Brush.IsNullOrEmpty(emptyRadialGradientBrush));

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

			Assert.False(Brush.IsNullOrEmpty(radialGradientBrush));
		}

		[Fact]
		public void TestRadialGradientBrushRadius()
		{
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush();
			radialGradientBrush.Radius = 20;

			Assert.Equal(20, radialGradientBrush.Radius);
		}

		[Fact]
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

			Assert.NotNull(radialGradientBrush);
		}

		[Fact]
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

			Assert.Equal(2, radialGradientBrush.GradientStops.Count);
		}
	}
}