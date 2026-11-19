using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxTests
	{
		bool? GetNativeCheckedState(CheckBoxHandler handler) =>
			handler.PlatformView.IsChecked;

		void ActivateNativeCheckBox(CheckBoxHandler handler)
		{
			var peer = new CheckBoxAutomationPeer(handler.PlatformView);
			var provider = (IToggleProvider)peer.GetPattern(PatternInterface.Toggle);
			if (handler.PlatformView.IsEnabled)
			{
				provider.Toggle();
			}
			else
			{
				var error = Record.Exception(() => provider.Toggle());
				Assert.NotNull(error);
				Assert.Equal(unchecked((int)0x80040200), error.HResult); // UIA_E_ELEMENTNOTENABLED
			}
		}

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
