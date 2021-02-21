using Xamarin.Forms;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using System.Threading.Tasks;
using System;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		ASwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(ASwitch)switchHandler.View;

		bool GetNativeIsChecked(SwitchHandler switchHandler) =>
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
				nativeSwitch.AssertContainsColor(color);
			});
		}
	}
}