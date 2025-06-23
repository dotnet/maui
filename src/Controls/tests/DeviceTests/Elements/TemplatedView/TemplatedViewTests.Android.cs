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
			var expected = templatedView.ScaleX;
			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a TemplatedView should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var templatedView = new TemplatedView() { ScaleY = 1.23f };
			var expected = templatedView.ScaleY;
			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a TemplatedView should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var templatedView = new TemplatedView() { Scale = 2.0f };
			var expected = templatedView.Scale;
			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a TemplatedView should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var templatedView = new TemplatedView() { RotationX = 33.0 };
			var expected = templatedView.RotationX;
			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
			var platformRotationX = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a TemplatedView should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var templatedView = new TemplatedView() { RotationY = 87.0 };
			var expected = templatedView.RotationY;
			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
			var platformRotationY = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a TemplatedView should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var templatedView = new TemplatedView() { Rotation = 23.0 };
			var expected = templatedView.Rotation;
			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);
			var platformRotation = await InvokeOnMainThreadAsync(() => handler.PlatformView.Rotation);
			Assert.Equal(expected, platformRotation);
		}
	}
}
