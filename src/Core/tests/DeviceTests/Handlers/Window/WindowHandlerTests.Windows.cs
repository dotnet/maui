using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : HandlerTestBase
	{

		[Fact(DisplayName = "Back Button Not Visible With No Navigation Page")]
		public async Task BackButtonNotVisibleWithBasicView()
		{
			var window = new WindowStub()
			{
				Content = new ButtonStub()
			};

			await RunWindowTest(window, manager =>
			{
				var navView = GetRootNavigationView(manager);
				Assert.Equal(NavigationViewBackButtonVisible.Collapsed, navView.IsBackButtonVisible);
				return Task.CompletedTask;
			});
		}


		RootNavigationView GetRootNavigationView(NavigationRootManager navigationRootManager)
		{
			return (navigationRootManager.RootView as WindowRootView).NavigationViewControl;
		}

		Task RunWindowTest(IWindow window, Func<NavigationRootManager, Task> action)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				FrameworkElement frameworkElement = null;
				var content = (Panel)DefaultWindow.Content;
				try
				{
					var scopedContext = new MauiContext(MauiContext.Services);
					scopedContext.AddWeakSpecific(DefaultWindow);
					var mauiContext = scopedContext.MakeScoped(true);
					var windowManager = mauiContext.GetNavigationRootManager();
					windowManager.Connect(window.Content);
					frameworkElement = windowManager.RootView;

					TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
					frameworkElement.Loaded += (_, __) => taskCompletionSource.SetResult(true);
					content.Children.Add(frameworkElement);
					await taskCompletionSource.Task;
					await action(windowManager);
				}
				finally
				{
					if (frameworkElement != null)
						content.Children.Remove(frameworkElement);
				}

				return Task.CompletedTask;
			});
		}
	}
}