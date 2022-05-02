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
				FrameworkElement frameworkElement = null;
				var content = (Panel)MauiProgram.DefaultWindow.Content;
				try
				{
					var mauiContext = MauiContext.MakeScoped(true);
					var handler = CreateHandler(navigationView, mauiContext);
					frameworkElement = handler.PlatformView;
					content.Children.Add(frameworkElement);
					if (navigationView is NavigationViewStub nvs && nvs.NavigationStack?.Count > 0)
					{
						navigationView.RequestNavigation(new NavigationRequest(nvs.NavigationStack, false));
						await nvs.OnNavigationFinished;
					}

					await action(handler);
				}
				finally
				{
					if (frameworkElement != null)
						content.Children.Remove(frameworkElement);
				}
			});
		}
	}
}