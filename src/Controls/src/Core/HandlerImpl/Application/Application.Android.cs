#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		public static void MapWindowSoftInputModeAdjust(ApplicationHandler handler, Application application)
		{
			Platform.ApplicationExtensions.UpdateWindowSoftInputModeAdjust(handler.PlatformView, application);
		}
	}
}