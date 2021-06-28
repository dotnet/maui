namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>, INativeWindowHandler
	{
		public static void MapTitle(WindowHandler handler, IWindow window) =>
			handler.NativeView?.UpdateTitle(window);
	}
}