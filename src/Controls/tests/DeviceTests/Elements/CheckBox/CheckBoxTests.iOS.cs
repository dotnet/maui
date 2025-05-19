using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		MauiCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task<float> GetPlatformOpacity(CheckBoxHandler checkBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(checkBoxHandler);
				return (float)nativeView.Alpha;
			});
		}

		[Fact("The IsEnabled of a CheckBox should match with native IsEnabled")]
		public async Task CheckBoxIsEnabled()
		{
			var checkBox = new CheckBox();
			checkBox.IsEnabled = false;
			var expectedValue = checkBox.IsEnabled;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var nativeView = GetNativeCheckBox(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		Task<bool> GetPlatformIsVisible(CheckBoxHandler checkBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(checkBoxHandler);
				return !nativeView.Hidden;
			});
		}
	}
}