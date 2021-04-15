using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		UISwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(UISwitch)switchHandler.NativeView;

		// This will not fire a ValueChanged event on native
		void SetIsOn(SwitchHandler switchHandler, bool value) =>
			switchHandler.NativeView.SetState(value, true);

		bool GetNativeIsOn(SwitchHandler switchHandler) =>
		  GetNativeSwitch(switchHandler).On;

		async Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(switchStub, handler =>
			{
				var native = GetNativeSwitch(handler);
				action?.Invoke();
				return native.OnTintColor.ToColor();
			});
			Assert.Equal(expected, color);
		}

		async Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(switchStub, handler =>
			{
				var native = GetNativeSwitch(handler);
				action?.Invoke();
				return native.ThumbTintColor.ToColor();
			});

			Assert.Equal(expected, color);
		}
	}
}