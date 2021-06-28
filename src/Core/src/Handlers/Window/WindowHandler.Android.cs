using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>, INativeWindowHandler
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }
	}
}