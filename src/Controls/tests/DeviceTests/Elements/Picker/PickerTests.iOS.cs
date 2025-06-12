using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	}
}