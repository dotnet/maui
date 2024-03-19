using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

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
