#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		internal static void MapUserAppTheme(ApplicationHandler handler, Application application)
		{
			application.UpdateNightMode();
		}

		public static void MapWindowSoftInputModeAdjust(ApplicationHandler handler, Application application)
		{
			Platform.ApplicationExtensions.UpdateWindowSoftInputModeAdjust(handler.PlatformView, application);
		}
	}
}