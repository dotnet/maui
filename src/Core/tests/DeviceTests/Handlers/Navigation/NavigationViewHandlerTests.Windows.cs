using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class NavigationViewHandlerTests
	{
		int GetNativeNavigationStackCount(NavigationViewHandler navigationViewHandler) =>
			navigationViewHandler.PlatformView.BackStackDepth + 1;

		Task CreateNavigationViewHandlerAsync(IStackNavigationView navigationView, Func<NavigationViewHandler, Task> action)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var mauiContext = MauiContext.MakeScoped(true);
				var handler = CreateHandler<NavigationViewHandler>(navigationView, MauiContext);
				await handler.PlatformView.AttachAndRunAsync(async () =>
				{
					if (navigationView is NavigationViewStub nvs && nvs.NavigationStack?.Count > 0)
					{
						navigationView.RequestNavigation(new NavigationRequest(nvs.NavigationStack, false));
						await nvs.OnNavigationFinished;
					}

					await action(handler);
				});
			});
		}
	}
}