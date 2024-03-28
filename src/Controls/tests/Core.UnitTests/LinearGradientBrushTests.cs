using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class LinearGradientBrushTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

			Assert.Equal(1.0d, linearGradientBrush.EndPoint.X);
			Assert.Equal(1.0d, linearGradientBrush.EndPoint.Y);
		}

		[Fact]
		public void TestConstructorUsingGradientStopCollection()
		{
			var gradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.Red, Offset = 0.1f },
				new GradientStop { Color = Colors.Orange, Offset = 0.8f }
			};

			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientStops, new Point(0, 0), new Point(0, 1));

			Assert.NotEmpty(linearGradientBrush.GradientStops);
			Assert.Equal(0.0d, linearGradientBrush.EndPoint.X);
			Assert.Equal(1.0d, linearGradientBrush.EndPoint.Y);
		}

		[Fact]
		public void TestEmptyLinearGradientBrush()
		{
			LinearGradientBrush nullLinearGradientBrush = new LinearGradientBrush();
			Assert.True(nullLinearGradientBrush.IsEmpty);

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

			Assert.False(linearGradientBrush.IsEmpty);
		}

		[Fact]
		public void TestNullOrEmptyLinearGradientBrush()
		{
			LinearGradientBrush nullLinearGradientBrush = null;
			Assert.True(Brush.IsNullOrEmpty(nullLinearGradientBrush));

			LinearGradientBrush emptyLinearGradientBrush = new LinearGradientBrush();
			Assert.True(Brush.IsNullOrEmpty(emptyLinearGradientBrush));

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

			Assert.False(Brush.IsNullOrEmpty(linearGradientBrush));
		}

		[Fact]
		public void TestNullOrEmptyLinearGradientPaintWithEmptyGradientStop()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop(),
					new GradientStop()
				}
			};

			Paint linearGradientPaint = linearGradientBrush;

			Assert.True(linearGradientPaint.IsNullOrEmpty());
		}

		[Fact]
		public void TestNullOrEmptyLinearGradientPaintWithNullGradientStop()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					null,
					null
				}
			};

			Paint linearGradientPaint = linearGradientBrush;

			Assert.True(linearGradientPaint.IsNullOrEmpty());
		}

		[Fact]
		public void TestNullGradientStopLinearGradientPaint()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, Offset = 0.1f },
					null,
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				}
			};

			Paint linearGradientPaint = linearGradientBrush;

			Assert.False(linearGradientPaint.IsNullOrEmpty());
		}

		[Fact]
		public void TestLinearGradientBrushPoints()
		{
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0)
			};

			Assert.Equal(0, linearGradientBrush.StartPoint.X);
			Assert.Equal(0, linearGradientBrush.StartPoint.Y);

			Assert.Equal(1, linearGradientBrush.EndPoint.X);
			Assert.Equal(0, linearGradientBrush.EndPoint.Y);
		}

		[Fact]
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

			Assert.NotNull(linearGradientBrush);
		}

		[Fact]
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

			Assert.Equal(2, linearGradientBrush.GradientStops.Count);
		}
	}
}