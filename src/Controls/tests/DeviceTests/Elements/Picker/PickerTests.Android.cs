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
            var handler = await CreateHandlerAsync<PickerHandler>(picker);
            var expected = picker.ScaleX;
            var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
            Assert.Equal(expected, platformScaleX);
        }

		[Fact]
		[Description("The ScaleY property of a Picker should match with native ScaleY")]
        public async Task ScaleYConsistent()
        {
            var picker = new Picker() { ScaleY = 0.45f };
            var handler = await CreateHandlerAsync<PickerHandler>(picker);
            var expected = picker.ScaleY;
            var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
            Assert.Equal(expected, platformScaleY);
        }
	}
}