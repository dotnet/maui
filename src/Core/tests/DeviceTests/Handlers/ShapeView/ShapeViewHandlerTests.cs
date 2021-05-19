using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ShapeView)]
	public partial class ShapeViewHandlerTests : HandlerTestBase<ShapeViewHandler, ShapeViewStub>
	{
		[Fact(DisplayName = "Rectangle Initializes Correctly")]
		public async Task RectangleInitializesCorrectly()
		{
			var rectangle = new ShapeViewStub()
			{
				Shape = new RectangleStub(),
				Fill = new SolidPaintStub(Colors.Red),
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(rectangle, Colors.Red);
		}

		[Fact(DisplayName = "Ellipse Initializes Correctly")]
		public async Task EllipseInitializesCorrectly()
		{
			var ellipse = new ShapeViewStub()
			{
				Shape = new EllipseStub(),
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
				Shape = new LineStub { X1 = 0, Y1 = 0, X2 = 90, Y2 = 45 },
				Stroke = Colors.Purple,
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
				Shape = new PolylineStub { Points = new PointCollection() { new Point(10, 10), new Point(100, 50), new Point(50, 90) } },
				Stroke = Colors.Green,
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
				Shape = new PolygonStub { Points = new PointCollection() { new Point(10, 10), new Point(100, 50), new Point(50, 90) } },
				Fill = new SolidPaintStub(Colors.Lime),
				Stroke = Colors.Black,
				StrokeThickness = 4,
				Height = 50,
				Width = 100
			};

			await ValidateNativeFill(polygon, Colors.Lime);
		}

		[Fact(DisplayName = "Path Initializes Correctly")]
		public async Task PathInitializesCorrectly()
		{
			var path = new ShapeViewStub()
			{
				Shape = new PathStub { Data = "M15.999996,0L31.999999,13.000001 15.999996,26.199999 0,13.000001z" },
				Fill = new SolidPaintStub(Colors.Coral),
				Stroke = Colors.Black,
				StrokeThickness = 1,
				Height = 50,
				Width = 50
			};

			await ValidateNativeFill(path, Colors.Coral);
		}
	}
}
