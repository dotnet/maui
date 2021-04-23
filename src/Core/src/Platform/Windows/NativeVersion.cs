using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public static class NativeVersion
	{
		public static bool IsDesktop =>
			Window.Current == null;
	}
}
