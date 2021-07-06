namespace Microsoft.Maui
{
	public static class WindowExtensions
	{
		public static void UpdateTitle(this UI.Xaml.Window nativeWindow, IWindow window) =>
			nativeWindow.Title = window.Title;
	}
}