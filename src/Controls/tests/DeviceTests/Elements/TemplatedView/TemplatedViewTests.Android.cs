using System.ComponentModel;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TemplatedViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler) =>
			contentViewHandler.PlatformView.ChildCount;

		static Android.Views.View GetChild(ContentViewHandler contentViewHandler, int index = 0) =>
			contentViewHandler.PlatformView.GetChildAt(index);

		[Fact]
		[Description("The ScaleX property of a TemplatedView should match with native ScaleX")]
        public async Task ScaleXConsistent()
        {
            var templatedView = new TemplatedView() { ScaleX = 0.45f };
            var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
            var expected = templatedView.ScaleX;
            var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
            Assert.Equal(expected, platformScaleX);
        }

		[Fact]
		[Description("The ScaleY property of a TemplatedView should match with native ScaleY")]
        public async Task ScaleYConsistent()
        {
            var templatedView = new TemplatedView() { ScaleY = 0.45f };
            var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
            var expected = templatedView.ScaleY;
            var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
            Assert.Equal(expected, platformScaleY);
        }
	}
}
