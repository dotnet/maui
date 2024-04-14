namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		internal static void MapUserAppTheme(ApplicationHandler handler, Application application)
		{
			application?.UpdateUserInterfaceStyle();
		}
	}
}