using System.Threading.Tasks;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;
using AViewStates = Android.Views.ViewStates;
using NativeShellHandler = Microsoft.Maui.Controls.Handlers.ShellHandler;

namespace Microsoft.Maui.DeviceTests
{
	public class StartupTrackingShellHandler : NativeShellHandler
	{
		public int TabLayoutAppearanceTrackerCreationCount { get; private set; }

		public int BottomNavAppearanceTrackerCreationCount { get; private set; }

		protected override IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection)
		{
			TabLayoutAppearanceTrackerCreationCount++;
			return base.CreateTabLayoutAppearanceTracker(shellSection);
		}

		protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
		{
			BottomNavAppearanceTrackerCreationCount++;
			return base.CreateBottomNavViewAppearanceTracker(shellItem);
		}
	}

	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellHandler)] // See RendererHandlerVariant.cs
	public partial class ShellHandlerTests_Shell : ShellTests
	{
		[Fact]
		public async Task SinglePageShellCreatesTabInfrastructureOnlyWhenNeeded()
		{
			SetupBuilder();

			var firstPage = new ContentPage();
			var firstContent = new ShellContent { Content = firstPage };
			var expectedBadgeText = "7";
			var section = new ShellSection
			{
				BadgeText = expectedBadgeText,
				Items = { firstContent }
			};
			var item = new FlyoutItem { Items = { section } };
			var secondSection = new ShellSection
			{
				Title = "Second section",
				Items = { new ShellContent { Content = new ContentPage() } }
			};
			var secondContent = new ShellContent
			{
				Title = "Second content",
				Content = new ContentPage()
			};
			var expectedTabBackground = Colors.Red;
			var shell = await CreateShellAsync(shell =>
			{
				Shell.SetBackgroundColor(shell, expectedTabBackground);
				shell.Items.Add(item);
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(firstPage);

				var shellHandler = Assert.IsType<StartupTrackingShellHandler>(shell.Handler);
				var itemHandler = Assert.IsType<ShellItemHandler>(item.Handler);
				var sectionHandler = Assert.IsType<ShellSectionHandler>(section.Handler);

				Assert.Null(itemHandler._tabbedViewManager);
				Assert.Null(itemHandler._shellItemAdapter);
				Assert.Null(itemHandler.BottomNavigationView);
				Assert.Null(sectionHandler.ContentTabLayout);
				Assert.Equal(0, shellHandler.BottomNavAppearanceTrackerCreationCount);
				Assert.Equal(0, shellHandler.TabLayoutAppearanceTrackerCreationCount);
				var topTabsContainer = shellHandler.PlatformView.FindViewById(Resource.Id.navigationlayout_toptabs);
				Assert.NotNull(topTabsContainer);
				Assert.Equal(AViewStates.Gone, topTabsContainer.Visibility);

				item.Items.Add(secondSection);
				section.Items.Add(secondContent);

				Assert.NotNull(itemHandler._tabbedViewManager);
				Assert.NotNull(sectionHandler.ContentTabLayout);
				var bottomBackground = Assert.IsType<ColorChangeRevealDrawable>(itemHandler.BottomNavigationView.Background);
				Assert.Equal(expectedTabBackground.ToPlatform(), bottomBackground.EndColor);
				var badge = itemHandler.BottomNavigationView.GetBadge(0);
				Assert.NotNull(badge);
				Assert.Equal(expectedBadgeText, badge.Text);
				var background = Assert.IsType<ColorDrawable>(sectionHandler.ContentTabLayout.Background);
				Assert.Equal(expectedTabBackground.ToPlatform(), background.Color);
				Assert.Equal(1, shellHandler.BottomNavAppearanceTrackerCreationCount);
				Assert.Equal(1, shellHandler.TabLayoutAppearanceTrackerCreationCount);

				var tabbedViewManager = itemHandler._tabbedViewManager;
				var shellItemAdapter = itemHandler._shellItemAdapter;
				var bottomNavigationView = itemHandler.BottomNavigationView;
				var contentTabLayout = sectionHandler.ContentTabLayout;

				item.Items.Remove(secondSection);
				section.Items.Remove(secondContent);
				Assert.Equal(AViewStates.Gone, topTabsContainer.Visibility);
				item.Items.Add(secondSection);
				section.Items.Add(secondContent);

				Assert.Same(tabbedViewManager, itemHandler._tabbedViewManager);
				Assert.Same(shellItemAdapter, itemHandler._shellItemAdapter);
				Assert.Same(bottomNavigationView, itemHandler.BottomNavigationView);
				Assert.Same(contentTabLayout, sectionHandler.ContentTabLayout);
				Assert.Equal(1, shellHandler.BottomNavAppearanceTrackerCreationCount);
				Assert.Equal(1, shellHandler.TabLayoutAppearanceTrackerCreationCount);
			});
		}

		[Fact]
		public async Task SwitchingShellItemsCreatesBottomTabsOnlyWhenNeeded()
		{
			SetupBuilder();

			var firstPage = new ContentPage();
			var firstItem = new FlyoutItem
			{
				Title = "First item",
				Items =
				{
					new ShellSection
					{
						Items = { new ShellContent { Content = firstPage } }
					}
				}
			};
			var secondPage = new ContentPage();
			var secondItem = new FlyoutItem
			{
				Title = "Second item",
				Items =
				{
					new ShellSection
					{
						Title = "First section",
						Items = { new ShellContent { Content = secondPage } }
					},
					new ShellSection
					{
						Title = "Second section",
						Items = { new ShellContent { Content = new ContentPage() } }
					}
				}
			};
			var expectedTabBackground = Colors.Blue;
			var shell = await CreateShellAsync(shell =>
			{
				Shell.SetTabBarBackgroundColor(secondItem, expectedTabBackground);
				shell.Items.Add(firstItem);
				shell.Items.Add(secondItem);
				shell.CurrentItem = firstItem;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(firstPage);

				var shellHandler = Assert.IsType<StartupTrackingShellHandler>(shell.Handler);
				Assert.Equal(0, shellHandler.BottomNavAppearanceTrackerCreationCount);

				shell.CurrentItem = secondItem;
				await OnLoadedAsync(secondPage);

				var itemHandler = Assert.IsType<ShellItemHandler>(secondItem.Handler);
				Assert.Equal(1, shellHandler.BottomNavAppearanceTrackerCreationCount);
				Assert.NotNull(itemHandler._tabbedViewManager);
				var background = Assert.IsType<ColorChangeRevealDrawable>(itemHandler.BottomNavigationView.Background);
				Assert.Equal(expectedTabBackground.ToPlatform(), background.EndColor);

				shell.CurrentItem = firstItem;
				await OnLoadedAsync(firstPage);

				itemHandler = Assert.IsType<ShellItemHandler>(firstItem.Handler);
				Assert.Equal(1, shellHandler.BottomNavAppearanceTrackerCreationCount);
				Assert.Null(itemHandler._tabbedViewManager);
				Assert.Null(itemHandler.BottomNavigationView);

				shell.CurrentItem = secondItem;
				await OnLoadedAsync(secondPage);

				itemHandler = Assert.IsType<ShellItemHandler>(secondItem.Handler);
				Assert.Equal(2, shellHandler.BottomNavAppearanceTrackerCreationCount);
				Assert.NotNull(itemHandler._tabbedViewManager);
			});
		}

		[Fact]
		public async Task SwitchingShellItemsRecreatesBottomNavAppearanceTracker()
		{
			SetupBuilder();

			var firstPage = new ContentPage();
			var firstItem = new FlyoutItem
			{
				Title = "First item",
				Items =
				{
					new ShellSection
					{
						Title = "First section",
						Items = { new ShellContent { Content = firstPage } }
					},
					new ShellSection
					{
						Title = "Second section",
						Items = { new ShellContent { Content = new ContentPage() } }
					}
				}
			};
			var secondPage = new ContentPage();
			var secondItem = new FlyoutItem
			{
				Title = "Second item",
				Items =
				{
					new ShellSection
					{
						Title = "First section",
						Items = { new ShellContent { Content = secondPage } }
					},
					new ShellSection
					{
						Title = "Second section",
						Items = { new ShellContent { Content = new ContentPage() } }
					}
				}
			};
			var expectedTabBackground = Colors.Blue;
			var shell = await CreateShellAsync(shell =>
			{
				Shell.SetTabBarBackgroundColor(firstItem, Colors.Red);
				Shell.SetTabBarBackgroundColor(secondItem, expectedTabBackground);
				shell.Items.Add(firstItem);
				shell.Items.Add(secondItem);
				shell.CurrentItem = firstItem;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(firstPage);

				var shellHandler = Assert.IsType<StartupTrackingShellHandler>(shell.Handler);
				Assert.Equal(1, shellHandler.BottomNavAppearanceTrackerCreationCount);

				shell.CurrentItem = secondItem;
				await OnLoadedAsync(secondPage);

				var itemHandler = Assert.IsType<ShellItemHandler>(secondItem.Handler);
				Assert.Equal(2, shellHandler.BottomNavAppearanceTrackerCreationCount);
				var background = Assert.IsType<ColorChangeRevealDrawable>(itemHandler.BottomNavigationView.Background);
				Assert.Equal(expectedTabBackground.ToPlatform(), background.EndColor);
			});
		}

		protected override void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					// Register all standard handlers first (Layout, Image, Label, Page, Toolbar, MenuBar, etc.)
					SetupShellHandlers(handlers);
					// Override Shell with the new NativeShellHandler
					handlers.AddHandler(typeof(Controls.Shell), typeof(StartupTrackingShellHandler));
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

		protected override void SetupShellTabColorsTest(Shell shell)
		{
			Shell.SetTabBarIsVisible(shell, true);
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
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellHandler)] // See RendererHandlerVariant.cs
	public partial class ModalHandlerTests : ModalTests
	{
		protected override void SetupBuilder(bool includeNavigationViewHandler = true)
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
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellHandler)] // See RendererHandlerVariant.cs
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
