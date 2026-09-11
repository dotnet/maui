using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using Google.Android.Material.Shape;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		// We only want to fire BackButtonVisible Toolbar events if the user
		// is changing the default behavior of the BackButtonVisibility
		// this way the platform animations are allowed to just happen naturally
		[Fact(DisplayName = "Pushing And Popping Doesnt Fire BackButtonVisible Toolbar Events")]
		public async Task PushingAndPoppingDoesntFireBackButtonVisibleToolbarEvents()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				bool failed = false;
				var toolbar = (NavigationPageToolbar)navPage.FindMyToolbar();
				toolbar.PropertyChanged += (_, args) =>
				{
					if (args.PropertyName == nameof(Toolbar.BackButtonVisible) ||
						args.PropertyName == nameof(Toolbar.DrawerToggleVisible))
					{
						failed = true;
					}
				};

				await navPage.Navigation.PushAsync(new ContentPage());
				Assert.False(failed);
				await navPage.Navigation.PopAsync();
				Assert.False(failed);
			});
		}

		[Fact(DisplayName = "StackNavigationManager Clears References On Disconnect (Issue 33918)")]
		public async Task StackNavigationManagerClearsReferencesOnDisconnect()
		{
			SetupBuilder();

			var mainPage = new ContentPage { Title = "Main Page" };
			var baseNavPage = new NavigationPage(mainPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(baseNavPage), async (handler) =>
			{
				var tab1Content = new ContentPage { Title = "Tab 1", Content = new Label { Text = "Tab 1 Content" } };
				var tab2Content = new ContentPage { Title = "Tab 2", Content = new Label { Text = "Tab 2 Content" } };

				var tab1Nav = new NavigationPage(tab1Content) { Title = "Tab 1" };
				var tab2Nav = new NavigationPage(tab2Content) { Title = "Tab 2" };

				var tabbedPage = new TabbedPage
				{
					Title = "Tabbed Modal",
					Children = { tab1Nav, tab2Nav }
				};

				await baseNavPage.Navigation.PushModalAsync(tabbedPage, animated: false);
				await OnLoadedAsync(tab1Content);

				var tab1Handler = tab1Nav.Handler as NavigationViewHandler;
				var tab2Handler = tab2Nav.Handler as NavigationViewHandler;
				Assert.NotNull(tab1Handler);
				Assert.NotNull(tab2Handler);

				var tab1SnManager = tab1Handler.StackNavigationManager;
				var tab2SnManager = tab2Handler.StackNavigationManager;
				Assert.NotNull(tab1SnManager);
				Assert.NotNull(tab2SnManager);

				tabbedPage.CurrentPage = tab2Nav;
				await OnLoadedAsync(tab2Content);

				await baseNavPage.Navigation.PopModalAsync(animated: false);

				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var currentPageField = typeof(StackNavigationManager).GetField("_currentPage", flags);
				var fragmentContainerViewField = typeof(StackNavigationManager).GetField("_fragmentContainerView", flags);
				var fragmentManagerField = typeof(StackNavigationManager).GetField("_fragmentManager", flags);

				Assert.NotNull(currentPageField);
				Assert.NotNull(fragmentContainerViewField);
				Assert.NotNull(fragmentManagerField);

				await AssertEventually(() =>
					currentPageField.GetValue(tab1SnManager) == null &&
					fragmentContainerViewField.GetValue(tab1SnManager) == null &&
					fragmentManagerField.GetValue(tab1SnManager) == null &&
					currentPageField.GetValue(tab2SnManager) == null &&
					fragmentContainerViewField.GetValue(tab2SnManager) == null &&
					fragmentManagerField.GetValue(tab2SnManager) == null,
					message: "StackNavigationManager fields were not cleared after Disconnect()");
			});
		}

		[Fact(DisplayName = "NavigationPage BarBackgroundColor colors AppBar status bar area")]
		public async Task BarBackgroundColorUpdatesAppBarStatusBarArea()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			var firstColor = Colors.Orange;
			var secondColor = Colors.Blue;
			var navPage = new NavigationPage(new ContentPage { Title = "Page Title" })
			{
				BarBackgroundColor = firstColor,
				BarTextColor = Colors.Black
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnLoadedAsync(navPage.CurrentPage);

				var platformToolbar = GetPlatformToolbar(handler.MauiContext);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();
				Assert.NotNull(appBar);

				AssertAppBarBackgroundColor(appBar, firstColor);

				navPage.BarBackgroundColor = secondColor;
				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == secondColor.ToPlatform().ToArgb());
			});
		}

		[Fact(DisplayName = "NavigationPage BarBackgroundColor colors Android status bar on initial load")]
		public async Task BarBackgroundColorUpdatesAndroidStatusBarOnInitialLoad()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			var firstColor = Colors.Orange;
			var secondColor = Colors.Blue;
			var navPage = new NavigationPage(new ContentPage { Title = "Page Title" })
			{
				BarBackgroundColor = firstColor,
				BarTextColor = Colors.Black
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnLoadedAsync(navPage.CurrentPage);

				var platformWindow = handler.PlatformView.Window;
				Assert.NotNull(platformWindow);

#pragma warning disable CA1422 // System bar color APIs still apply to older Android versions and are harmless on newer versions.
				await AssertEventually(() => platformWindow.StatusBarColor == firstColor.ToPlatform().ToArgb());

				navPage.BarBackgroundColor = secondColor;
				await AssertEventually(() => platformWindow.StatusBarColor == secondColor.ToPlatform().ToArgb());
#pragma warning restore CA1422
			});
		}

		[Fact(DisplayName = "NavigationPage BarBackgroundColor respects Android system chrome opt-out")]
		public async Task BarBackgroundColorDoesNotUpdateAndroidSystemChromeWhenRuntimeFeatureDisabled()
		{
			if (RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			var expectedSystemBarColor = Colors.Purple;
			var barBackgroundColor = Colors.Orange;
			var navPage = new NavigationPage(new ContentPage { Title = "Page Title" })
			{
				BarTextColor = Colors.Black
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnLoadedAsync(navPage.CurrentPage);

				var platformToolbar = GetPlatformToolbar(handler.MauiContext);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();
				Assert.NotNull(appBar);

				var platformWindow = handler.PlatformView.Window;
				Assert.NotNull(platformWindow);

#pragma warning disable CA1422 // System bar color APIs still apply to older Android versions and are harmless on newer versions.
				var originalStatusBarColor = platformWindow.StatusBarColor;

				try
				{
					var originalAppBarBackgroundColor = GetAppBarBackgroundColor(appBar);
					platformWindow.SetStatusBarColor(expectedSystemBarColor.ToPlatform());

					navPage.BarBackgroundColor = barBackgroundColor;

					Assert.Equal(originalAppBarBackgroundColor, GetAppBarBackgroundColor(appBar));
					Assert.Equal(expectedSystemBarColor.ToPlatform().ToArgb(), platformWindow.StatusBarColor);
				}
				finally
				{
					platformWindow.SetStatusBarColor(new global::Android.Graphics.Color(originalStatusBarColor));
				}
#pragma warning restore CA1422
			});
		}

		static int GetAppBarBackgroundColor(AppBarLayout appBar)
		{
			if (ViewCompat.GetBackgroundTintList(appBar) is { } backgroundTint)
			{
				return backgroundTint.GetColorForState(
					appBar.GetDrawableState(),
					new global::Android.Graphics.Color(backgroundTint.DefaultColor));
			}

			return appBar.Background switch
			{
				ColorDrawable colorDrawable => colorDrawable.Color.ToArgb(),
				MaterialShapeDrawable materialShapeDrawable when materialShapeDrawable.FillColor is not null =>
					materialShapeDrawable.FillColor.GetColorForState(
						appBar.GetDrawableState(),
						new global::Android.Graphics.Color(materialShapeDrawable.FillColor.DefaultColor)),
				_ => throw new XunitException($"Expected AppBar background to be {nameof(ColorDrawable)} or {nameof(MaterialShapeDrawable)}, but was {appBar.Background?.GetType().FullName ?? "null"}.")
			};
		}

		static void AssertAppBarBackgroundColor(AppBarLayout appBar, Color expectedColor)
		{
			Assert.Equal(expectedColor.ToPlatform().ToArgb(), GetAppBarBackgroundColor(appBar));
		}

		[Fact(DisplayName = "NavigationPage push to hidden navigation bar clears app bar inset padding")]
		public async Task PushingToPageWithoutNavigationBarClearsAppBarInsetPadding()
		{
			SetupBuilder();
			const int statusBarTopInset = 24;
			const int displayCutoutTopInset = 96;

			var rootPage = new ContentPage
			{
				Title = "Visible Page",
				Content = new Label { Text = "Root Content" }
			};

			var hiddenNavBarPage = new ContentPage
			{
				Title = "Hidden Page",
				Content = new Label { Text = "Hidden Content" }
			};

			NavigationPage.SetHasNavigationBar(hiddenNavBarPage, false);

			var syntheticInsets = CreateTopCutoutInsets(statusBarTopInset, displayCutoutTopInset);
			var navPage = new NavigationPage(rootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnLoadedAsync(rootPage);
				await OnNavigatedToAsync(rootPage);

				var rootCoordinator = handler.MauiContext?.GetNavigationRootManager()?.RootView as CoordinatorLayout;
				Assert.NotNull(rootCoordinator);

				MaterialToolbar platformToolbar = null;
				await AssertEventually(() =>
				{
					platformToolbar = GetPlatformToolbar(handler);
					return platformToolbar?.LayoutParameters?.Height > 0;
				},
					timeout: 2000,
					message: "Toolbar did not render before navigating to the page with the navigation bar hidden.");

				Assert.NotNull(platformToolbar);
				var appBar = rootCoordinator.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
				Assert.NotNull(appBar);
				var capturingListener = AttachCapturingWindowInsetsListener(rootCoordinator, platformToolbar);

				ViewCompat.DispatchApplyWindowInsets(rootCoordinator, syntheticInsets);

				await AssertEventually(() => capturingListener.InvocationCount > 0,
					timeout: 2000,
					message: "The NavigationPage root did not receive the initial synthetic window insets dispatch.");

				await AssertEventually(() => appBar.PaddingTop == displayCutoutTopInset,
					timeout: 2000,
					message: "AppBar never received the synthetic display cutout top inset while the NavigationPage navigation bar was visible.");

				AssertTopInsets(capturingListener.LastAppliedInsets, expectedSystemBarsTop: 0, expectedDisplayCutoutTop: 0,
					message: "Visible NavigationPage app bar should consume the synthetic top insets.");

				var visibleInsetsInvocationCount = capturingListener.InvocationCount;

				await navPage.Navigation.PushAsync(hiddenNavBarPage);
				await OnLoadedAsync(hiddenNavBarPage);
				await OnNavigatedToAsync(hiddenNavBarPage);

				await AssertEventually(() =>
				{
					var currentToolbar = GetPlatformToolbar(handler);
					return currentToolbar?.LayoutParameters?.Height == 0 && currentToolbar.Height == 0;
				},
					timeout: 2000,
					message: "Toolbar did not fully collapse after navigating to a page with NavigationPage.HasNavigationBar set to false.");

				await AssertEventually(() => capturingListener.InvocationCount > visibleInsetsInvocationCount && appBar.PaddingTop == 0,
					timeout: 2000,
					message: "Navigating to a page with the navigation bar hidden did not trigger an inset redispatch that cleared the AppBar top padding.");

				// Re-dispatch the synthetic insets now that the nav bar is hidden so we can assert
				// with known values that the app bar no longer consumes the top insets.
				var hiddenInsetsInvocationCount = capturingListener.InvocationCount;
				ViewCompat.DispatchApplyWindowInsets(rootCoordinator, syntheticInsets);

				await AssertEventually(() => capturingListener.InvocationCount > hiddenInsetsInvocationCount,
					timeout: 2000,
					message: "Expected an additional inset dispatch after re-injecting synthetic insets post-nav-bar-hide.");

				AssertTopInsets(capturingListener.LastAppliedInsets,
					expectedSystemBarsTop: statusBarTopInset,
					expectedDisplayCutoutTop: displayCutoutTopInset,
					message: "Hidden NavigationPage app bar should stop consuming the synthetic top insets.");
			});
		}
	}
}
