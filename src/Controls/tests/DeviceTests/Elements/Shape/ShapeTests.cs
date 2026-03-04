using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shape)]
	public partial class ShapeTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Button, ButtonHandler>();

					handlers.AddHandler<Line, LineHandler>();
				});
			});
		}

		[Fact(DisplayName = "Line PathForBounds should produce symmetric paths for mirror-image lines")]
		public void LinePathForBoundsShouldBeSymmetricForMirrorLines()
		{
			// Two lines forming a V shape: line1 from top-left to center, line2 from top-right to center
			var line1 = new Line { X1 = 0, Y1 = 0, X2 = 100, Y2 = 100, StrokeThickness = 10 };
			var line2 = new Line { X1 = 200, Y1 = 0, X2 = 100, Y2 = 100, StrokeThickness = 10 };

			var viewBounds = new Graphics.Rect(0, 0, 200, 200);
			var path1 = ((IShape)line1).PathForBounds(viewBounds);
			var path2 = ((IShape)line2).PathForBounds(viewBounds);

			var bounds1 = path1.GetBoundsByFlattening(1);
			var bounds2 = path2.GetBoundsByFlattening(1);

			// line1 and line2 should be mirror images around center X=100
			// So: bounds1.Left + bounds2.Right ≈ 200 (left edges are symmetric)
			// And: bounds1.Right + bounds2.Left ≈ 200 (right edges are symmetric)
			const float tolerance = 2f;
			Assert.True(
				Math.Abs(bounds1.Left + bounds2.Right - 200f) < tolerance,
				$"X-axis symmetry failed: bounds1.Left={bounds1.Left:F1} + bounds2.Right={bounds2.Right:F1} should equal 200");
			Assert.True(
				Math.Abs(bounds1.Right + bounds2.Left - 200f) < tolerance,
				$"X-axis symmetry failed: bounds1.Right={bounds1.Right:F1} + bounds2.Left={bounds2.Left:F1} should equal 200");

			// Y bounds should be identical for both lines (same Y transformation)
			Assert.True(
				Math.Abs(bounds1.Top - bounds2.Top) < tolerance,
				$"Y-axis top mismatch: bounds1.Top={bounds1.Top:F1} vs bounds2.Top={bounds2.Top:F1}");
			Assert.True(
				Math.Abs(bounds1.Bottom - bounds2.Bottom) < tolerance,
				$"Y-axis bottom mismatch: bounds1.Bottom={bounds1.Bottom:F1} vs bounds2.Bottom={bounds2.Bottom:F1}");
		}

		[Theory(DisplayName = "Shape Updates brush Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task ShapeUpdatesBrushCorrectly(uint color)
		{
			SetupBuilder();

			var expected = Color.FromUint(color);

			var layout = new StackLayout();

			var line = new Line
			{
				X1 = 0,
				Y1 = 0,
				X2 = 100,
				Y2 = 100,
				HeightRequest = 100,
				WidthRequest = 100,
				Stroke = Colors.Black,
				StrokeThickness = 4
			};

			var button = new Button()
			{
				Text = "Update Fill Color"
			};

			layout.Add(line);
			layout.Add(button);

			var clicked = false;

			var pathGeometry2 = new PathGeometry();

			button.Clicked += delegate
			{
				clicked = true;
				line.Stroke = expected;
			};

			await AttachAndRun<LineHandler>(line, async (handler) =>
			{
				await PerformClick(button);

				Assert.True(clicked);

				await AssertEventually(
					() =>
					handler.PlatformView is not null &&
					handler.PlatformView.Drawable is not null);

				var mauiShapeView = handler.PlatformView;
				Assert.NotNull(mauiShapeView);
				var shapeDrawable = mauiShapeView.Drawable as ShapeDrawable;
				Assert.NotNull(shapeDrawable);
				var shape = shapeDrawable.ShapeView.Shape as Shape;
				Assert.NotNull(shape);

				var shapeStroke = shape.Stroke as SolidColorBrush;
				Assert.Equal(expected, shapeStroke?.Color);
			});
		}
	}
}