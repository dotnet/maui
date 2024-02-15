using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		void SetIsOn(SwitchHandler switchHandler, bool value) =>
			GetNativeSwitch(switchHandler).IsOn = value;

		ToggleSwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(ToggleSwitch)switchHandler.PlatformView;

		bool GetNativeIsOn(SwitchHandler switchHandler) =>
			GetNativeSwitch(switchHandler).IsOn;

		async Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null)
		{
			await IdleSynchronizer.GetForCurrentProcess().WaitAsync();
			await ValidateHasColor(switchStub, color, action, updatePropertyValue: updatePropertyValue);
		}

		async Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null)
		{
			await IdleSynchronizer.GetForCurrentProcess().WaitAsync();
			await ValidateHasColor(switchStub, color, action, updatePropertyValue: updatePropertyValue);
		}
	}
}