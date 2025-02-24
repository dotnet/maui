using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.ComponentModel;
using Xunit;
using Microsoft.Maui.Controls;
using System.Diagnostics;
namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewViewHandler) =>
			boxViewViewHandler.PlatformView;
		 
		Task<int> GetPlatformCornerRadius(BoxViewHandler boxViewHandler)
        {
            return InvokeOnMainThreadAsync(() =>
            {
                var platformView = GetNativeBoxView(boxViewHandler);
                if (platformView.Background is Android.Graphics.Drawables.GradientDrawable gradientDrawable)
                {
                    return (int)gradientDrawable.CornerRadius;
                }
                return 0;
            });
        }

		

		[Fact]
        [Description("The CornerRadius of a BoxView should match with native CornerRadius")]
        public async Task BoxViewCornerRadius()
        {
            var boxView = new BoxView
            {
                Color = Colors.Red,
                CornerRadius = 15
            };
            var expectedValue = boxView.CornerRadius;

            var handler = await CreateHandlerAsync<BoxViewHandler>(boxView);

            await InvokeOnMainThreadAsync(async () =>
            {
                var platformCornerRadius = await GetPlatformCornerRadius(handler);
                Assert.Equal(expectedValue, platformCornerRadius);
            });
        }
	}
}