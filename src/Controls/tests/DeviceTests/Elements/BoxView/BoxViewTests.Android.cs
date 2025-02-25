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
using Android.Graphics.Drawables;
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
		[Description("The IsEnabled property of a BoxView should match with native IsEnabled")]		
		public async Task VerifyBoxViewIsEnabledProperty()
		{
			var boxView = new BoxView
			{
				IsEnabled = false
			};
			var expectedValue = boxView.IsEnabled;

			var handler = await CreateHandlerAsync<BoxViewHandler>(boxView);
			var nativeView = GetNativeBoxView(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});		
		}
	}
}