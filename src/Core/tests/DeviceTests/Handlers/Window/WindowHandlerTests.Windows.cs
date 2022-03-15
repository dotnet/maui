using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WBorder = Microsoft.UI.Xaml.Controls.Border;
using WVisibility = Microsoft.UI.Xaml.Visibility;

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


		[Theory(DisplayName = "MauiToolbar Control Visibilities Toggle")]
		[InlineData("titleIcon")]
		[InlineData("textBlockBorder")]
		[InlineData("menuContent")]
		[InlineData("titleView")]
		public async Task MauiToolbarControlVisibilitiesToggle(string controlName)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				MauiToolbar mauiToolbar = new MauiToolbar();
				var toolbarContent = (DependencyObject)mauiToolbar.Content;
				var control = toolbarContent.GetDescendantByName<UIElement>(controlName);

				Assert.Equal(WVisibility.Collapsed, control.Visibility);

				switch (controlName)
				{
					case "titleIcon":
						var tcs = new TaskCompletionSource<bool>();
						var fileImageSource = new Controls.FileImageSource() { File = "black.png" };
						fileImageSource.LoadImage(MauiContext, (result) =>
						{
							mauiToolbar.TitleIconImageSource = result.Value;
							tcs.SetResult(true);
						});

						await tcs.Task;
						Assert.Equal(WVisibility.Visible, control.Visibility);
						mauiToolbar.TitleIconImageSource = null;
						Assert.Equal(WVisibility.Collapsed, control.Visibility);
						break;
					case "textBlockBorder":
						mauiToolbar.Title = "text";
						Assert.Equal(WVisibility.Visible, control.Visibility);
						mauiToolbar.Title = "";
						Assert.Equal(WVisibility.Collapsed, control.Visibility);
						break;
					case "menuContent":
						mauiToolbar.SetMenuBar(new MenuBar() { Items = { new MenuBarItem() } });
						Assert.Equal(WVisibility.Visible, control.Visibility);
						mauiToolbar.SetMenuBar(new MenuBar());
						Assert.Equal(WVisibility.Collapsed, control.Visibility);
						mauiToolbar.SetMenuBar(new MenuBar() { Items = { new MenuBarItem() } });
						Assert.Equal(WVisibility.Visible, control.Visibility);
						mauiToolbar.SetMenuBar(null);
						Assert.Equal(WVisibility.Collapsed, control.Visibility);
						break;
					case "titleView":
						mauiToolbar.TitleView = "text";
						Assert.Equal(WVisibility.Visible, control.Visibility);
						mauiToolbar.TitleView = null;
						Assert.Equal(WVisibility.Collapsed, control.Visibility);
						break;
				}
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