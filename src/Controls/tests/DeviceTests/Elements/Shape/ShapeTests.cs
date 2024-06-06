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