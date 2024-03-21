using System;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase
	{
		Task SetupWindowForTests<THandler>(IWindow window, Func<Task> runTests, IMauiContext mauiContext = null)
			where THandler : class, IElementHandler
		{
			mauiContext ??= MauiContext;
			return InvokeOnMainThreadAsync(async () =>
			{
				IElementHandler windowHandler = null;
				try
				{
					windowHandler = window.ToHandler(mauiContext);
					await runTests.Invoke();
				}
				finally
				{
					window.Handler?.DisconnectHandler();

					var vc =
						(window.Content?.Handler as IPlatformViewHandler)?
							.ViewController;

					vc?.RemoveFromParentViewController();
					vc?.View?.RemoveFromSuperview();
				}
			});
		}
	}
}
