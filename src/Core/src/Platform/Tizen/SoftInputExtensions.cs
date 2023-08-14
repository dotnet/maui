using AutoCapital = Tizen.NUI.InputMethod.AutoCapitalType;
using TKeyboard = Tizen.UIExtensions.Common.Keyboard;
using PlatformView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static partial class SoftInputExtensions
	{
		internal static bool HideSoftInput(this PlatformView view) => SetKeyInputFocus(view, false);

		internal static bool ShowSoftInput(this PlatformView view) => SetKeyInputFocus(view, true);

		internal static bool IsSoftInputShowing(this PlatformView view)
		{
			return view.KeyInputFocus;
		}

		internal static bool SetKeyInputFocus(PlatformView view, bool isShow)
		{
			view.KeyInputFocus = isShow;

			return view.KeyInputFocus;
		}
	}
}