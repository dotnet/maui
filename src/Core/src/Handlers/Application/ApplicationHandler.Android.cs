using Android.App;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Application>
	{
		public static void MapRequestTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.Logger?.LogWarning("Android does not support programatically terminating the app.");
		}
	}
}