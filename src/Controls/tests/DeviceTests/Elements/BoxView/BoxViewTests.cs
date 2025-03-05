using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.BoxView)]
	public partial class BoxViewTests : ControlsHandlerTestBase
	{
		[Theory(DisplayName = "BoxView Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BoxViewInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var boxView = new BoxView()
			{
				Color = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(boxView, expected, typeof(ShapeViewHandler));
		}

		[Fact]
		[Description("The BackgroundColor of a BoxView should match with native background color")]
		public async Task BoxViewBackgroundColorConsistent()
		{
			var expected = Colors.AliceBlue;

			var boxView = new BoxView()
			{
				BackgroundColor = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(boxView, expected, typeof(ShapeViewHandler));
		}
	}
}