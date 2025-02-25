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