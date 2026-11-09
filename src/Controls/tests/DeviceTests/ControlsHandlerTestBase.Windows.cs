using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Xunit;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WAppBarButton = Microsoft.UI.Xaml.Controls.AppBarButton;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
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
				var applicationContext = mauiContext.MakeApplicationScope(UI.Xaml.Application.Current);

				var appStub = new MauiAppNewWindowStub(window);
				UI.Xaml.Application.Current.SetApplicationHandler(appStub, applicationContext);
				WWindow newWindow = null;
				MauiContext windowContext = null;
				try
				{
					ApplicationExtensions.CreatePlatformWindow(UI.Xaml.Application.Current, appStub, new Handlers.OpenWindowRequest());
					newWindow = window.Handler.PlatformView as WWindow;
					windowContext = window.Handler.MauiContext as MauiContext;
					await runTests.Invoke();
				}
				finally
				{
					// Disconnecting clears Window.Handler before Destroying can dispose its scope.
					windowContext ??= window.Handler?.MauiContext as MauiContext;
					try
					{
						window.Handler?.DisconnectHandler();
						await Task.Delay(10);
						var windowHandle = newWindow?.GetWindowHandle() ?? IntPtr.Zero;
						newWindow?.Close();
						if (windowHandle != IntPtr.Zero)
						{
							using var process = Process.GetCurrentProcess();
							Console.WriteLine($"NATIVE_WINDOW_CLEANUP hwnd=0x{windowHandle.ToInt64():X} alive={IsNativeWindow(windowHandle)} processHandles={process.HandleCount} gdiObjects={GetGuiResources(process.Handle, 0)} userObjects={GetGuiResources(process.Handle, 1)}");
						}
					}
					finally
					{
						try
						{
							windowContext?.DisposeWindowScope();
						}
						finally
						{
							appStub.Handler?.DisconnectHandler();
						}
					}
				}
			});
		}

		[DllImport("user32.dll", EntryPoint = "IsWindow", ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsNativeWindow(IntPtr windowHandle);

		[DllImport("user32.dll", ExactSpelling = true)]
		static extern uint GetGuiResources(IntPtr processHandle, uint flags);

		protected IEnumerable<WNavigationViewItem> GetNavigationViewItems(MauiNavigationView navigationView)
		{
			if (navigationView.MenuItems?.Count > 0)
			{
				foreach (var menuItem in navigationView.MenuItems)
				{
					if (menuItem is WNavigationViewItem item)
						yield return item;
				}
			}
			else if (navigationView.MenuItemsSource != null && navigationView.TopNavMenuItemsHost != null)
			{
				var itemCount = navigationView.TopNavMenuItemsHost.ItemsSourceView.Count;
				for (int i = 0; i < itemCount; i++)
				{
					UI.Xaml.UIElement uIElement = navigationView.TopNavMenuItemsHost.TryGetElement(i);

					if (uIElement is WNavigationViewItem item)
						yield return item;
				}
			}
		}

		protected double DistanceYFromTheBottomOfTheAppTitleBar(IElement element)
		{
			var handler = element.Handler;
			var rootManager = handler.MauiContext.GetNavigationRootManager();
			var position = element.GetLocationRelativeTo(rootManager.AppTitleBar);
			var distance = rootManager.AppTitleBar.ActualHeight - position.Value.Y;
			return distance;
		}

		MauiNavigationView GetMauiNavigationView(NavigationRootManager navigationRootManager)
		{
			return (navigationRootManager.RootView as WindowRootView).NavigationViewControl;
		}

		protected WindowRootView GetWindowRootView(IElementHandler handler)
		{
			return handler.MauiContext.GetNavigationRootManager().RootView as WindowRootView;
		}

		protected MauiNavigationView GetMauiNavigationView(IMauiContext mauiContext)
		{
			return GetMauiNavigationView(mauiContext.GetNavigationRootManager());
		}

		protected bool IsBackButtonVisible(IElementHandler handler) =>
			IsBackButtonVisible(handler.MauiContext);

		bool IsBackButtonVisible(IMauiContext mauiContext)
		{
			var navView = GetMauiNavigationView(mauiContext);
			return navView.IsBackButtonVisible == UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
		}

		public bool IsNavigationBarVisible(IElementHandler handler) =>
			IsNavigationBarVisible(handler.MauiContext);

		public bool IsNavigationBarVisible(IMauiContext mauiContext)
		{
			var header = GetPlatformToolbar(mauiContext);
			return header?.Visibility == UI.Xaml.Visibility.Visible;
		}

		protected MauiToolbar GetPlatformToolbar(IMauiContext mauiContext)
		{
			var navView = (RootNavigationView)GetMauiNavigationView(mauiContext);
			if (navView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
				return (MauiToolbar)navView.PaneFooter;

			return (MauiToolbar)navView.Header;
		}

		protected MauiToolbar GetPlatformToolbar(IElementHandler handler) =>
			GetPlatformToolbar(handler.MauiContext);

		protected Size GetTitleViewExpectedSize(IElementHandler handler)
		{
			var headerView = GetPlatformToolbar(handler.MauiContext);
			return new Size(headerView.ActualWidth, headerView.ActualHeight);
		}

		public bool ToolbarItemsMatch(
			IElementHandler handler,
			params ToolbarItem[] toolbarItems)
		{
			var primaryToolbarItems = toolbarItems.Where(x => x.Order != ToolbarItemOrder.Secondary).ToArray();
			var secondaryToolbarItems = toolbarItems.Where(x => x.Order == ToolbarItemOrder.Secondary).ToArray();

			var navView = (RootNavigationView)GetMauiNavigationView(handler.MauiContext);
			MauiToolbar windowHeader = (MauiToolbar)navView.Header;

			ValidateCommandBarCommands(windowHeader?.CommandBar?.PrimaryCommands, primaryToolbarItems);
			ValidateCommandBarCommands(windowHeader?.CommandBar?.SecondaryCommands, secondaryToolbarItems);

			void ValidateCommandBarCommands(IObservableVector<ICommandBarElement> commands, ToolbarItem[] orderToolbarItems)
			{
				if (orderToolbarItems.Length == 0)
				{
					Assert.True(commands is null || commands.Count == 0);
					return;
				}

				Assert.NotNull(commands);
				Assert.Equal(orderToolbarItems.Length, commands.Count);
				for (var i = 0; i < toolbarItems.Length; i++)
				{
					ToolbarItem toolbarItem = orderToolbarItems[i];
					var command = ((WAppBarButton)commands[i]);
					Assert.Equal(toolbarItem, command.DataContext);
				}
			}

			return true;
		}

		protected FrameworkElement GetTitleView(IElementHandler handler)
		{
			var toolbar = GetPlatformToolbar(handler);
			return (FrameworkElement)toolbar.TitleView;
		}

		protected string GetToolbarTitle(IElementHandler handler) =>
			GetPlatformToolbar(handler).Title;
	}
}
