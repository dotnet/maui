using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
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

		[Fact]
		[Description("The Background of a BoxView should match with native Background")]
		public async Task BoxViewBackgroundConsistent()
		{
			var boxView = new BoxView
			{
				HeightRequest = 100,
				WidthRequest = 200,
				Background = Brush.Red
			};
			var expected = (boxView.Background as SolidColorBrush)?.Color;

			await ValidateHasColor(boxView, expected, typeof(BoxViewHandler));
		}

		[Fact]
		[Description("The Opacity property of a BoxView should match with native Opacity")]
		public async Task VerifyBoxViewOpacityProperty()
		{
			var boxView = new BoxView
			{
				Opacity = 0.35f
			};
			var expectedValue = boxView.Opacity;

			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

		[Fact]
		[Description("The IsVisible property of a BoxView should match with native IsVisible")]
		public async Task VerifyBoxViewIsVisibleProperty()
		{
			var boxView = new BoxView();
			boxView.IsVisible = false;
			var expectedValue = boxView.IsVisible;

			var handler = await CreateHandlerAsync<BoxViewHandler>(boxView);
			await InvokeOnMainThreadAsync(async () =>
   			{
				   var isVisible = await GetPlatformIsVisible(handler);
				   Assert.Equal(expectedValue, isVisible);
			   });
		}
	}
}