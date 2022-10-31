using System;
using System.Threading.Tasks;
using Android.Graphics;
using Microsoft.Maui.Handlers;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		void SetIsOn(SwitchHandler switchHandler, bool value) =>
			GetNativeSwitch(switchHandler).Checked = value;

		ASwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(ASwitch)switchHandler.PlatformView;

		bool GetNativeIsOn(SwitchHandler switchHandler) =>
			GetNativeSwitch(switchHandler).Checked;

		Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null) =>
			ValidateHasColor(switchStub, color, action);

		Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null) =>
			ValidateHasColor(switchStub, color, action);

		Task ValidateHasColor(ISwitch switchStub, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeSwitch = GetNativeSwitch(CreateHandler(switchStub));
				action?.Invoke();
				return nativeSwitch.AssertContainsColorAsync(color);
			});
		}
	}
}