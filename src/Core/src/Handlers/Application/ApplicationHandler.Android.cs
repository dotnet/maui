using Android.App;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Application>
	{
		public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.Logger?.LogWarning("Android does not support programmatically terminating the app.");
		}
	}
}