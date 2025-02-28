using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewViewHandler) =>
			boxViewViewHandler.PlatformView;

		Task<float> GetPlatformOpacity(ShapeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeBoxView(handler);
				return nativeView.Alpha;
			});
		}
        
		[Fact]
		[Description("The ScaleX property of a BoxView should match with native ScaleX")]
        public async Task ScaleXConsistent()
        {
            var boxView = new BoxView() { ScaleX = 0.45f };
            var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
            var expected = boxView.ScaleX;
            var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
            Assert.Equal(expected, platformScaleX);
        }

        [Fact]
		[Description("The ScaleY property of a BoxView should match with native ScaleY")]
        public async Task ScaleYConsistent()
        {
            var boxView = new BoxView() { ScaleY = 0.45f };
            var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
            var expected = boxView.ScaleY;
            var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
            Assert.Equal(expected, platformScaleY);
        }
	}
}