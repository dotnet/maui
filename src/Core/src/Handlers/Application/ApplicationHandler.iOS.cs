using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, UIApplicationDelegate>
	{
		public static void MapRequestTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.Logger?.LogWarning("iOS does not support programatically terminating the app.");
		}
	}
}