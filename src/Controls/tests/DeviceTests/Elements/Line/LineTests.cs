using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Line)]
	public partial class LineTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<StackLayout, LayoutHandler>();
					handlers.AddHandler<Line, LineHandler>();
				});
			});
		}

		[Theory(DisplayName = "Line using Stroke Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task StrokeInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var line = new Line()
			{
				Stroke = expected,
				X1 = 0,
				Y1 = 0,
				X2 = 200,
				Y2 = 5,
				HeightRequest = 10,
				WidthRequest = 200
			};

			await ValidateHasColor(line, expected, typeof(LineHandler));
		}

		[Theory(DisplayName = "Line using BackgroundColor Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundColorInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var line = new Line()
			{
				BackgroundColor = expected,
				X1 = 0,
				Y1 = 0,
				X2 = 200,
				Y2 = 5,
				HeightRequest = 10,
				WidthRequest = 200
			};

			await ValidateHasColor(line, expected, typeof(LineHandler));
		}
	}
}
