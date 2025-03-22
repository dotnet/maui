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
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		MauiSwipeView GetPlatformControl(SwipeViewHandler handler) =>
			handler.PlatformView;

		Task<bool> HasChildren(SwipeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(()
				=> GetPlatformControl(handler).Subviews.Length != 0);
		}

		[Fact]
		[Description("The Opacity property of a SwipeView should match with native Opacity")]
		public async Task VerifySwipeViewOpacityProperty()
		{
			var swipeView = new SwipeView
			{
				Opacity = 0.35f
			};
			var expectedValue = swipeView.Opacity;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var nativeOpacityValue = (float)nativeView.Alpha;
				   Assert.Equal(expectedValue, nativeOpacityValue);
			   });
		}
	}
}

