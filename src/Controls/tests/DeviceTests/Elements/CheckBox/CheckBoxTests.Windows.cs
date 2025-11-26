using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxTests
	{
		CheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task<float> GetPlatformOpacity(CheckBoxHandler checkBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(checkBoxHandler);
				return (float)nativeView.Opacity;
			});
		}

		[Fact("The IsEnabled of a CheckBox should match with native IsEnabled")]
		public async Task CheckBoxIsEnabled()
		{
			var checkBox = new Microsoft.Maui.Controls.CheckBox();
			checkBox.IsEnabled = false;
			var expectedValue = checkBox.IsEnabled;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var nativeView = GetNativeCheckBox(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.IsEnabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		Task<bool> GetPlatformIsVisible(CheckBoxHandler checkBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(checkBoxHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}
	}
}
