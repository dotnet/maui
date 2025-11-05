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

		[Fact]
		public void TestHasTransparencyRadialGradientBrush()
		{
			RadialGradientBrush nullRadialGradientBrush = null;
			Assert.False(Brush.HasTransparency(nullRadialGradientBrush));

			RadialGradientBrush emptyRadialGradientBrush = new RadialGradientBrush();
			Assert.False(Brush.HasTransparency(emptyRadialGradientBrush));

			RadialGradientBrush brushWithNullStops = new RadialGradientBrush
			{
				GradientStops = null
			};
			Assert.False(Brush.HasTransparency(brushWithNullStops));

			RadialGradientBrush opaqueBrush = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, Offset = 0.0f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				}
			};
			Assert.False(Brush.HasTransparency(opaqueBrush));

			RadialGradientBrush transparentBrush = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Transparent, Offset = 0.0f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				}
			};
			Assert.True(Brush.HasTransparency(transparentBrush));

			RadialGradientBrush mixedBrush = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, Offset = 0.0f },
					new GradientStop { Color = Color.FromRgba(0, 255, 0, 0.5), Offset = 0.5f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				}
			};
			Assert.True(Brush.HasTransparency(mixedBrush));

			RadialGradientBrush allSemiTransparentBrush = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Color.FromRgba(255, 0, 0, 0.3), Offset = 0.0f },
					new GradientStop { Color = Color.FromRgba(0, 0, 255, 0.7), Offset = 1.0f }
				}
			};
			Assert.True(Brush.HasTransparency(allSemiTransparentBrush));
		}
	}
}