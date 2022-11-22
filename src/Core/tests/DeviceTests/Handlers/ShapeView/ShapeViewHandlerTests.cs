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
		[Theory(DisplayName = "Shape Background Initializes Correctly")]
		[InlineData(0xFF0000)]
		[InlineData(0x00FF00)]
		[InlineData(0x0000FF)]
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
	}
}
