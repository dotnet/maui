using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Border)]
	public partial class BorderHandlerTests : CoreHandlerTestBase<BorderHandler, BorderStub>
	{
		[Theory(DisplayName = "Background Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Background", TextColor = Colors.White },
				Shape = new RectangleShapeStub(),
				Background = new SolidPaintStub(expected),
				Stroke = new SolidPaintStub(Colors.Black),
				StrokeThickness = 2,
				Height = 100,
				Width = 300
			};

			await ValidateHasColor(border, expected);
		}

		[Theory(DisplayName = "Stroke Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task StrokeInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Stroke", TextColor = Colors.Black },
				Shape = new RectangleShapeStub(),
				Background = new SolidPaintStub(Colors.White),
				Stroke = new SolidPaintStub(expected),
				StrokeThickness = 6,
				Height = 100,
				Width = 300
			};

			await ValidateHasColor(border, expected);
		}

		[Theory(DisplayName = "Dashed Stroke Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task DashedStrokeInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Stroke", TextColor = Colors.Black },
				Shape = new RectangleShapeStub(),
				Background = new SolidPaintStub(Colors.White),
				Stroke = new SolidPaintStub(expected),
				StrokeDashPattern = new float[2] { 1, 1 },
				StrokeThickness = 6,
				Height = 100,
				Width = 300
			};

			await ValidateHasColor(border, expected);
		}

		[Theory(DisplayName = "StrokeShape Initializes Correctly")]
		[InlineData("Ellipse")]
		[InlineData("Rectangle")]
		[InlineData("RoundRectangle")]
		public async Task StrokeShapeInitializesCorrectly(string shape)
		{
			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "StrokeShape", TextColor = Colors.Black },
				Background = new SolidPaintStub(Colors.Red),
				Stroke = new SolidPaintStub(Colors.Black),
				StrokeThickness = 6,
				Height = 100,
				Width = 300
			};

			if (shape == "Ellipse")
			{
				border.Shape = new EllipseShapeStub();
			}

			if (shape == "Rectangle")
			{
				border.Shape = new RectangleShapeStub();
			}

			if (shape == "RoundRectangle")
			{
				border.Shape = new RoundRectangleShapeStub { CornerRadius = new CornerRadius(12, 0, 0, 24) };
			}

			await ValidateHasColor(border, Colors.Red);
		}
	}
}