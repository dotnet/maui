using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using AndroidX.DrawerLayout.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;
using NativeShellHandler = Microsoft.Maui.Controls.Handlers.ShellHandler;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class ShellHandlerTests_Shell : ShellTests
	{
		protected override void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					// Register all standard handlers first (Layout, Image, Label, Page, Toolbar, MenuBar, etc.)
					SetupShellHandlers(handlers);
					// Override Shell with the new NativeShellHandler
					handlers.AddHandler(typeof(Controls.Shell), typeof(NativeShellHandler));
					handlers.AddHandler(typeof(ShellItem), typeof(ShellItemHandler));
					handlers.AddHandler(typeof(ShellSection), typeof(ShellSectionHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
					handlers.AddHandler(typeof(Controls.ContentView), typeof(ContentViewHandler));
					handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewHandler));
					handlers.AddHandler(typeof(CollectionView), typeof(CollectionViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
				});
			});
		}

		// NativeShellHandler uses MauiDrawerLayout (not ShellFlyoutRenderer), so cast to MauiDrawerLayout.
		protected override DrawerLayout GetDrawerLayout(IShellContext shellContext)
		{
			return (MauiDrawerLayout)shellContext.CurrentDrawerLayout;
		}

		// The base IsBackButtonVisible uses GetPlatformToolbar which has no NativeShellHandler branch.
		// NativeShellHandler nests the toolbar inside an outer CoordinatorLayout — walk up to find it.
		protected override bool IsBackButtonVisible(IElementHandler handler)
		{
			if (GetShellHandlerToolbar(handler)?.NavigationIcon is DrawerArrowDrawable drawerArrow)
				return drawerArrow.Progress == 1;

			return false;
		}

		MaterialToolbar GetShellHandlerToolbar(IElementHandler handler)
		{
			// Direct NativeShellHandler: toolbar lives in nested CoordinatorLayout of shell.CurrentPage.
			if (handler is NativeShellHandler nativeShell)
			{
				var shell = nativeShell.VirtualView as Shell;
				var currentPage = shell?.CurrentPage;

				if (currentPage?.Handler?.PlatformView is AView pagePlatformView)
				{
					// Walk up CoordinatorLayouts — handler has nested coordinators;
					// the toolbar lives in the outer one (navigationlayout.axml).
					var coordinator = pagePlatformView.GetParentOfType<CoordinatorLayout>();
					while (coordinator is not null)
					{
						var toolbar = coordinator.GetFirstChildOfType<MaterialToolbar>();
						if (toolbar is not null)
							return toolbar;

						coordinator = (coordinator.Parent as AView)?.GetParentOfType<CoordinatorLayout>();
					}
				}

				return null;
			}

			// For page/navigation handlers (NavigationViewHandler, PageHandler, etc.):
			// use the handler's own MauiContext directly so modal pages find their own toolbar,
			// not the Shell's toolbar via the window content handler.
			return GetPlatformToolbar(handler.MauiContext);
		}
	}

	[Category(TestCategory.Modal)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class ModalHandlerTests : ModalTests
	{
		protected override void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					// Register all standard handlers first
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler(typeof(Controls.Shell), typeof(NativeShellHandler));
					handlers.AddHandler(typeof(ShellItem), typeof(ShellItemHandler));
					handlers.AddHandler(typeof(ShellSection), typeof(ShellSectionHandler));
					handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewHandler));
				});
			});
		}

		// Modal pages pushed over Shell need platform-view traversal to find the modal's own toolbar.
		// GetPlatformToolbar(MauiContext) resolves to the Shell's NavigationRootManager and finds
		// the wrong toolbar (Shell nav toolbar with back button from secondPage).
		protected override bool IsBackButtonVisible(IElementHandler handler)
		{
			// For NavigationPage handlers, check its current page's view hierarchy.
			VisualElement visualElement = handler.VirtualView as VisualElement;
			if (visualElement is NavigationPage navPage)
				visualElement = navPage.CurrentPage;

			if (visualElement?.Handler?.PlatformView is AView platformView)
			{
				// Walk up CoordinatorLayouts starting from the page's platform view.
				// This stays within the modal's view hierarchy, not the Shell's.
				var coordinator = platformView.GetParentOfType<CoordinatorLayout>();
				while (coordinator is not null)
				{
					var toolbar = coordinator.GetFirstChildOfType<MaterialToolbar>();
					if (toolbar?.NavigationIcon is DrawerArrowDrawable drawerArrow)
						return drawerArrow.Progress == 1;

					coordinator = (coordinator.Parent as AView)?.GetParentOfType<CoordinatorLayout>();
				}
			}

			return false;
		}
	}

	[Category(TestCategory.Window)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class WindowHandlerTests : WindowTests
	{
		protected override void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					// Register all standard handlers first
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(Controls.Shell), typeof(NativeShellHandler));
					handlers.AddHandler(typeof(ShellItem), typeof(ShellItemHandler));
					handlers.AddHandler(typeof(ShellSection), typeof(ShellSectionHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(Controls.ContentView), typeof(ContentViewHandler));
					handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewHandler));
				});
			});
		}
	}
}
