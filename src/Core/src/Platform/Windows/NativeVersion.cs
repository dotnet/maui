using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public static class NativeVersion
	{
		// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.window.current?view=winui-3.0
		// The currently activated window for UWP apps. Null for Desktop apps.
		public static bool IsDesktop =>
			Window.Current == null;
	}
}
