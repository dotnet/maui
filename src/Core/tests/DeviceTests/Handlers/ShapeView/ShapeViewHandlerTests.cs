using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ShapeView)]
	public partial class ShapeViewHandlerTests : CoreHandlerTestBase<ShapeViewHandler, ShapeViewStub>
	{
		[Fact(DisplayName = "Shadow Initializes Correctly on Shapes",
			Skip = "This test is currently invalid https://github.com/dotnet/maui/issues/13692")]
		public async Task ShadowInitializesCorrectly()
		{
			var xPlatShadow = new ShadowStub
			{
				Offset = new Point(10, 10),
				Opacity = 1.0f,
				Radius = 2.0f
			};

			var rectangle = new RectangleStub
			{
				Height = 50,
				Width = 50
			};

			rectangle.Shadow = xPlatShadow;

			await ValidateHasColor(rectangle, Colors.Red, () => xPlatShadow.Paint = new SolidPaint(Colors.Red));
		}

		[Theory(DisplayName = "Shape Background Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var rectangle = new ShapeViewStub()
			{
				Shape = new RectangleShapeStub(),
				Stroke = new SolidPaintStub(Colors.Red),
				StrokeThickness = 2,
				Background = new SolidPaintStub(expected),
				Height = 100,
				Width = 100
			};

			await ValidateHasColor(rectangle, expected);
		}

		[Fact(DisplayName = "Rectangle Initializes Correctly")]
		public async Task RectangleInitializesCorrectly()
		{
			var rectangle = new ShapeViewStub()
			{
				Shape = new RectangleShapeStub(),
				Fill = new SolidPaintStub(Colors.Red),
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(rectangle, Colors.Red);
		}

		[Fact(DisplayName = "RoundRectangle Initializes Correctly")]
		public async Task RoundRectangleInitializesCorrectly()
		{
			var rectangle = new ShapeViewStub()
			{
				Shape = new RoundRectangleShapeStub(),
				Fill = new SolidPaintStub(Colors.Orange),
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(rectangle, Colors.Orange);
		}

		[Fact(DisplayName = "Ellipse Initializes Correctly")]
		public async Task EllipseInitializesCorrectly()
		{
			var ellipse = new ShapeViewStub()
			{
				Shape = new EllipseShapeStub(),
				Fill = new SolidPaintStub(Colors.Blue),
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(ellipse, Colors.Blue);
		}

		[Fact(DisplayName = "Line Initializes Correctly")]
		public async Task LineInitializesCorrectly()
		{
			var line = new ShapeViewStub()
			{
				Shape = new LineShapeStub { X1 = 0, Y1 = 0, X2 = 90, Y2 = 45 },
				Stroke = new SolidPaintStub(Colors.Purple),
				StrokeThickness = 4,
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(line, Colors.Purple);
		}

		[Fact(DisplayName = "Polyline Initializes Correctly")]
		public async Task PolylineInitializesCorrectly()
		{
			var polyline = new ShapeViewStub()
			{
				Shape = new PolylineShapeStub { Points = new PointCollectionStub() { new Point(10, 10), new Point(100, 50), new Point(50, 90) } },
				Stroke = new SolidPaintStub(Colors.Green),
				StrokeThickness = 4,
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(polyline, Colors.Green);
		}

		[Fact(DisplayName = "Polygon Initializes Correctly")]
		public async Task PolygonInitializesCorrectly()
		{
			var polygon = new ShapeViewStub()
			{
				Shape = new PolygonShapeStub { Points = new PointCollectionStub() { new Point(10, 10), new Point(100, 50), new Point(50, 90) } },
				Fill = new SolidPaintStub(Colors.Lime),
				Stroke = new SolidPaintStub(Colors.Black),
				StrokeThickness = 4,
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(polygon, Colors.Lime);
		}

		[Theory(DisplayName = "Polyline Background Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task PolylineBackgroundInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var polyline = new ShapeViewStub()
			{
				Shape = new PolylineShapeStub { Points = new PointCollectionStub() { new Point(10, 10), new Point(100, 50), new Point(50, 90) } },
				Stroke = new SolidPaintStub(Colors.Green),
				Background = new SolidPaintStub(expected),
				StrokeThickness = 4,
				Height = 50,
				Width = 100
			};

			await ValidateHasColor(polyline, expected);
		}

		[Theory]
		[ClassData(typeof(IntrinsicSizeTestCases))]
		public async Task ShapesDoNotHaveIntrinsicSize(IShapeView shape)
		{
			var size = await InvokeOnMainThreadAsync(() => CreateHandler(shape).GetDesiredSize(100, 100));
			Assert.Equal(0, size.Width);
			Assert.Equal(0, size.Height);
		}
	}

	public class IntrinsicSizeTestCases : IEnumerable<object[]>
	{
		public IntrinsicSizeTestCases()
		{
			var rectangle = new ShapeViewStub()
			{
				Shape = new RectangleShapeStub(),
				Fill = new SolidPaintStub(Colors.Red),
				Height = double.NaN, // Have to explicitly reset this because StubBase sets H,W to 50/50
				Width = double.NaN
			};

			var polygon = new ShapeViewStub()
			{
				Shape = new PolygonShapeStub { Points = new PointCollectionStub() { new Point(10, 10), new Point(100, 50), new Point(50, 90) } },
				Fill = new SolidPaintStub(Colors.Lime),
				Stroke = new SolidPaintStub(Colors.Black),
				StrokeThickness = 4,
				Height = double.NaN,
				Width = double.NaN
			};

			var line = new ShapeViewStub()
			{
				Shape = new LineShapeStub { X1 = 0, Y1 = 0, X2 = 90, Y2 = 0 },
				Stroke = new SolidPaintStub(Colors.Purple),
				StrokeThickness = 0.6,
				Height = double.NaN,
				Width = double.NaN
			};

			_data = new()
			{
				new object[] { rectangle },
				new object[] { polygon },
				new object[] { line }
			};
		}

		private readonly List<object[]> _data;

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
