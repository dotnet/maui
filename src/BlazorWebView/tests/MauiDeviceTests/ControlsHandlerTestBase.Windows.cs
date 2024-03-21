using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using WWindow = Microsoft.UI.Xaml.Window;

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
				var appStub = new MauiAppNewWindowStub(window);

				WWindow newWindow = null;
				try
				{
					ApplicationExtensions.CreatePlatformWindow(UI.Xaml.Application.Current, appStub, new Handlers.OpenWindowRequest());
					newWindow = window.Handler.PlatformView as WWindow;
					await runTests.Invoke();
				}
				finally
				{
					window.Handler?.DisconnectHandler();
					await Task.Delay(10);
					newWindow?.Close();
					appStub.Handler?.DisconnectHandler();
				}
			});
		}
	}
}
