using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		protected Task<string> GetPlatformControlText(MauiPicker platformView)
		{
			return InvokeOnMainThreadAsync(() => platformView.Text);
		}

		MauiPicker GetPlatformPicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		Task<float> GetPlatformOpacity(PickerHandler pickerHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformPicker(pickerHandler);
				return (float)nativeView.Alpha;
			});
		}

		[Fact]
		[Description("The ScaleX property of a Picker should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var picker = new Picker() { ScaleX = 0.45f };
			var expected = picker.ScaleX;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Picker should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var picker = new Picker() { ScaleY = 1.23f };
			var expected = picker.ScaleY;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Picker should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var picker = new Picker() { Scale = 2.0f };
			var expected = picker.Scale;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Picker should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var picker = new Picker() { RotationX = 33.0 };
			var expected = picker.RotationX;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformRotationX = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Picker should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var picker = new Picker() { RotationY = 87.0 };
			var expected = picker.RotationY;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformRotationY = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Picker should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var picker = new Picker() { Rotation = 23.0 };
			var expected = picker.Rotation;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformRotation = await InvokeOnMainThreadAsync(() => handler.PlatformView.Rotation);
			Assert.Equal(expected, platformRotation);
		}
	}
}