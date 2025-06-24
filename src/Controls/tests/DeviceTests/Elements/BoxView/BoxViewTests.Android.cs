using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
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
			var expected = boxView.ScaleX;
			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var platformBoxView = GetNativeBoxView(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformBoxView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a BoxView should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var boxView = new BoxView() { ScaleY = 1.23f };
			var expected = boxView.ScaleY;
			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var platformBoxView = GetNativeBoxView(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformBoxView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a BoxView should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var boxView = new BoxView() { Scale = 2.0f };
			var expected = boxView.Scale;
			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var platformBoxView = GetNativeBoxView(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformBoxView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformBoxView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a BoxView should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var boxView = new BoxView() { RotationX = 33.0 };
			var expected = boxView.RotationX;
			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var platformBoxView = GetNativeBoxView(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformBoxView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a BoxView should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var boxView = new BoxView() { RotationY = 87.0 };
			var expected = boxView.RotationY;
			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var platformBoxView = GetNativeBoxView(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformBoxView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a BoxView should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var boxView = new BoxView() { Rotation = 23.0 };
			var expected = boxView.Rotation;
			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var platformBoxView = GetNativeBoxView(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformBoxView.Rotation);
			Assert.Equal(expected, platformRotation);
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

		Task<bool> GetPlatformIsVisible(ShapeViewHandler boxViewViewHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeBoxView(boxViewViewHandler);
				return nativeView.Visibility == Android.Views.ViewStates.Visible;
			});
		}
	}
}