using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Badge;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TabbedPageTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task BadgePropertiesUpdateNativeTab(bool bottomTabs)
		{
			SetupBuilder();

			var firstPage = new ContentPage { Title = "First" };
			var secondPage = new ContentPage { Title = "Second" };
			TabbedPage.SetBadgeText(firstPage, "7");
			TabbedPage.SetBadgeColor(firstPage, Colors.Blue);
			TabbedPage.SetBadgeTextColor(firstPage, Colors.Yellow);

			var tabbedPage = CreateBasicTabbedPage(bottomTabs, pages: new[] { firstPage, secondPage });

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				BadgeDrawable GetBadge() => bottomTabs
					? tabbedPage.TabbedPageManager.BottomNavigationView.GetBadge(0)
					: tabbedPage.TabbedPageManager.TabLayout.GetTabAt(0).Badge;

				await AssertEventually(() => GetBadge()?.Text == "7", message: "Initial badge text was not applied.");
				var badge = GetBadge();
				Assert.Equal(Colors.Blue.ToPlatform(), badge.BackgroundColor);
				Assert.Equal(Colors.Yellow.ToPlatform(), badge.BadgeTextColor);

				TabbedPage.SetBadgeText(firstPage, "");
				await AssertEventually(() => string.IsNullOrEmpty(badge.Text), message: "Badge did not change to a dot.");

				TabbedPage.SetBadgeText(firstPage, "New");
				TabbedPage.SetBadgeColor(firstPage, Colors.Orange);
				TabbedPage.SetBadgeTextColor(firstPage, Colors.Black);
				await AssertEventually(() =>
					badge.Text == "New" &&
					badge.BackgroundColor == Colors.Orange.ToPlatform() &&
					badge.BadgeTextColor == Colors.Black.ToPlatform(),
					message: "Badge text or colors did not update.");

				TabbedPage.SetBadgeTextColor(firstPage, null);
				await AssertEventually(() =>
					GetBadge() is { } resetBadge &&
					!ReferenceEquals(resetBadge, badge) &&
					resetBadge.Text == "New" &&
					resetBadge.BackgroundColor == Colors.Orange.ToPlatform(),
					message: "Clearing the badge text color did not restore the platform default.");

				TabbedPage.SetBadgeText(firstPage, null);
				await AssertEventually(() => bottomTabs
					? tabbedPage.TabbedPageManager.BottomNavigationView.GetBadge(0) is null
					: tabbedPage.TabbedPageManager.TabLayout.GetTabAt(0).Badge is null,
					message: "Badge was not removed.");
			});
		}

		[Fact]
		public async Task BottomTabBadgesRespectOverflow()
		{
			SetupBuilder();

			var pages = Enumerable.Range(0, 6)
				.Select(index =>
				{
					var page = new ContentPage { Title = $"Page {index}" };
					TabbedPage.SetBadgeText(page, index.ToString());
					return page;
				})
				.ToArray();
			var tabbedPage = CreateBasicTabbedPage(bottomTabs: true, pages: pages);

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				var bottomNavigationView = tabbedPage.TabbedPageManager.BottomNavigationView;

				await AssertEventually(() =>
					Enumerable.Range(0, 4).All(index =>
						bottomNavigationView.GetBadge(index)?.Text == index.ToString()),
					message: "Visible bottom tabs did not receive their badges.");

				Assert.Null(bottomNavigationView.GetBadge(4));
				Assert.Null(bottomNavigationView.GetBadge(5));
				Assert.Null(bottomNavigationView.GetBadge(BottomNavigationViewUtils.MoreTabId));
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task BadgeFollowsPageWhenTabIndexChanges(bool bottomTabs)
		{
			SetupBuilder();

			var firstPage = new ContentPage { Title = "First" };
			var badgedPage = new ContentPage { Title = "Badged" };
			TabbedPage.SetBadgeText(badgedPage, "4");
			var tabbedPage = CreateBasicTabbedPage(bottomTabs, pages: new[] { firstPage, badgedPage });

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				string GetBadgeText(int index) => bottomTabs
					? tabbedPage.TabbedPageManager.BottomNavigationView.GetBadge(index)?.Text
					: tabbedPage.TabbedPageManager.TabLayout.GetTabAt(index)?.Badge?.Text;

				await AssertEventually(() => GetBadgeText(1) == "4");

				tabbedPage.Children.Insert(0, new ContentPage { Title = "Inserted" });
				await AssertEventually(() =>
					GetBadgeText(1) is null &&
					GetBadgeText(2) == "4",
					message: "The badge did not follow its page after inserting a tab.");
			});
		}

		[Fact]
		[Description("TabbedPage BarBackgroundColor should be applied to AppBar immediately after handler creation, before the fragment transaction completes")]
		public async Task TopTabbedPageBarBackgroundColorAppliedOnInitialLoad()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			// Set BarBackgroundColor before the handler is created to exercise the path where
			// UpdateTopChrome is called during initialization (async fragment transaction).
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.BarBackgroundColor = Colors.LightGreen;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				var appBar = GetAppBarLayout(handler);
				Assert.NotNull(appBar);

				// The AppBar background must be applied even on initial load (timing race fix).
				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == Colors.LightGreen.ToPlatform().ToArgb());
			});
		}

		[Fact(DisplayName = "Using SelectedTab Color doesnt crash")]
		public async Task SelectedTabColorNoDoesntCrash()
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.SelectedTabColor = Colors.Red;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), (handler) =>
			{
				var platformView = tabbedPage.Handler.PlatformView as ViewPager2;
				Assert.NotNull(platformView);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task SettingJustSelectedATabColorOnBottomTabsDoesntCrash()
		{
			SetupBuilder();
			var tabbedPage = new TabbedPage
			{
				Children =
				{
					new ContentPage() { Title = "Page1"}
					,new ContentPage() { Title = "Page2"}
					,new ContentPage() { Title = "Page3"}

				},
				SelectedTabColor = Colors.Red,
			};

			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage
				.SetToolbarPlacement(tabbedPage, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

			tabbedPage.SelectedTabColor = Colors.Red;

			bool success = false;
			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				success = true;
			});

			Assert.True(success);
		}

		[Fact]
		public async Task ChangingBottomTabAttributesDoesntRecreateBottomTabs()
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage(true, pages: new[]
			{
				new ContentPage() { Title = "Tab 1", IconImageSource = "red.png" },
				new ContentPage() { Title = "Tab 2", IconImageSource = "red.png" }
			});

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async (handler) =>
			{
				var menu = GetBottomNavigationView(handler).Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var icon1 = menuItem1.Icon;
				var icon2 = menuItem2.Icon;
				var title1 = menuItem1.TitleFormatted;
				var title2 = menuItem2.TitleFormatted;

				tabbedPage.Children[0].Title = "new Title 1";
				tabbedPage.Children[0].IconImageSource = "blue.png";

				tabbedPage.Children[1].Title = "new Title 2";
				tabbedPage.Children[1].IconImageSource = "blue.png";

				// let the icon and title propagate
				await AssertEventually(() => menuItem1.Icon != icon1);

				menu = GetBottomNavigationView(handler).Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menuItem1.Icon.AssertColorAtCenter(global::Android.Graphics.Color.Blue);
				menuItem2.Icon.AssertColorAtCenter(global::Android.Graphics.Color.Blue);

				Assert.NotEqual(icon1, menuItem1.Icon);
				Assert.NotEqual(icon2, menuItem2.Icon);
				Assert.NotEqual(title1, menuItem1.TitleFormatted);
				Assert.NotEqual(title2, menuItem2.TitleFormatted);
			});
		}

		[Fact(DisplayName = "Custom RecyclerView Adapter Doesn't Crash")]
		public async Task CustomRecyclerViewAdapterDoesNotCrash()
		{
			SetupBuilder(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<TabbedPage, CustomTestAdapterHandler>();
				});
			});

			var tabbedPage = CreateBasicTabbedPage();

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				// If you currently try to modify the children too early.
				// This will sometimes cause `NotifyDataSourceChanged` on the
				// adapter to get called while it's already processing
				await Task.Delay(50);

				tabbedPage.Children.Add(new ContentPage());

				// make sure changes have time to propagate
				await Task.Delay(50);
			});
		}

		[Fact]
		[Description("The ScaleX property of a TabbedPage should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var tabbedPage = new TabbedPage() { ScaleX = 0.45f };
			var expected = tabbedPage.ScaleX;
			var handler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a TabbedPage should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var tabbedPage = new TabbedPage() { ScaleY = 1.23f };
			var expected = tabbedPage.ScaleY;
			var handler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a TabbedPage should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var tabbedPage = new TabbedPage() { Scale = 2.0f };
			var expected = tabbedPage.Scale;
			var handler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a TabbedPage should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var tabbedPage = new TabbedPage() { RotationX = 33.0 };
			var expected = tabbedPage.RotationX;
			var handler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);
			var platformRotationX = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a TabbedPage should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var tabbedPage = new TabbedPage() { RotationY = 87.0 };
			var expected = tabbedPage.RotationY;
			var handler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);
			var platformRotationY = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a TabbedPage should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var tabbedPage = new TabbedPage() { Rotation = 23.0 };
			var expected = tabbedPage.Rotation;
			var handler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);
			var platformRotation = await InvokeOnMainThreadAsync(() => handler.PlatformView.Rotation);
			Assert.Equal(expected, platformRotation);
		}

		[Fact]
		[Description("Top TabbedPage BarBackgroundColor should color the AppBar status bar area")]
		public async Task TopTabbedPageBarBackgroundColorColorsAppBarStatusBarArea()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			var firstColor = Colors.Orange;
			var secondColor = Colors.Blue;
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.BarBackgroundColor = firstColor;
			tabbedPage.BarTextColor = Colors.Black;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				var appBar = GetAppBarLayout(handler);
				Assert.NotNull(appBar);

				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == firstColor.ToPlatform().ToArgb());

				tabbedPage.BarBackgroundColor = secondColor;
				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == secondColor.ToPlatform().ToArgb());
			});
		}

		[Fact]
		[Description("Top TabbedPage BarBackground should restore the native AppBar drawable when changing from gradient to solid color")]
		public async Task TopTabbedPageBarBackgroundRestoresNativeDrawableWhenChangingFromGradientToSolid()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			var solidColor = Colors.Blue;
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.BarBackground = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Orange, Offset = 0 },
					new GradientStop { Color = Colors.Purple, Offset = 1 },
				}
			};
			tabbedPage.BarTextColor = Colors.Black;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				var appBar = GetAppBarLayout(handler);
				Assert.NotNull(appBar);

				await AssertEventually(() => appBar.Background is GradientStrokeDrawable);

				tabbedPage.BarBackground = new SolidColorBrush(solidColor);

				await AssertEventually(() =>
					appBar.Background is not GradientStrokeDrawable &&
					GetAppBarBackgroundColor(appBar) == solidColor.ToPlatform().ToArgb());
			});
		}

		[Fact]
		[Description("Top TabbedPage BarBackgroundColor should still update after the active AppBar background drawable state is disposed")]
		public async Task TopTabbedPageBarBackgroundColorUpdatesAfterAppBarBackgroundStateDisposed()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();

			var firstColor = Colors.Orange;
			var secondColor = Colors.Blue;
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.BarBackgroundColor = firstColor;
			tabbedPage.BarTextColor = Colors.Black;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				var appBar = GetAppBarLayout(handler);
				Assert.NotNull(appBar);

				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == firstColor.ToPlatform().ToArgb());

				var activeBackground = appBar.Background;
				var activeBackgroundState = activeBackground?.GetConstantState();
				appBar.Background = null;
				activeBackgroundState?.Dispose();
				activeBackground?.Dispose();

				tabbedPage.BarBackgroundColor = secondColor;

				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == secondColor.ToPlatform().ToArgb());
			});
		}

		[Fact]
		[Description("A bottom chrome update made before the native view is attached should be applied after attachment")]
		public async Task BottomChromeUpdateBeforeAttachReplaysAfterAttachment()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds || OperatingSystem.IsAndroidVersionAtLeast(35))
				return;

			SetupBuilder();

			var expectedColor = Colors.Green;
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(new ContentPage()), async handler =>
			{
				var platformWindow = handler.PlatformView.Window;
				Assert.NotNull(platformWindow);
				var content = handler.PlatformView.FindViewById<global::Android.Views.ViewGroup>(global::Android.Resource.Id.Content);
				Assert.NotNull(content);
				var chromeView = new BottomNavigationView(handler.PlatformView);

#pragma warning disable CA1422 // This test only runs where Android still honors system bar color APIs.
				var originalColor = platformWindow.NavigationBarColor;
				platformWindow.SetNavigationBarColor(Colors.Red.ToPlatform());

				try
				{
					AndroidSystemChrome.UpdateBottomChrome(chromeView, new SolidColorBrush(expectedColor));
					Assert.Equal(Colors.Red.ToPlatform().ToArgb(), platformWindow.NavigationBarColor);

					content.AddView(chromeView);
					await AssertEventually(() => platformWindow.NavigationBarColor == expectedColor.ToPlatform().ToArgb());
				}
				finally
				{
					content.RemoveView(chromeView);
					platformWindow.SetNavigationBarColor(new global::Android.Graphics.Color(originalColor));
					chromeView.Dispose();
				}
#pragma warning restore CA1422
			});
		}

		[Fact]
		[Description("BottomNavigationView should extend to screen bottom in Edge-to-Edge mode (Issue 33344)")]
		public async Task BottomNavigationViewExtendsToScreenBottom()
		{
			SetupBuilder();

			var tabbedPage = new TabbedPage
			{
				Children =
				{
					new ContentPage() { Title = "Page1" },
					new ContentPage() { Title = "Page2" }
				},
				BarBackgroundColor = Colors.Orange
			};

			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage
				.SetToolbarPlacement(tabbedPage, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				var bottomNavView = GetBottomNavigationView(handler);
				Assert.NotNull(bottomNavView);

				// Wait for layout to complete
				await AssertEventually(() => bottomNavView.Height > 0);

				var location = new int[2];
				bottomNavView.GetLocationOnScreen(location);
				var bottomNavBottom = location[1] + bottomNavView.Height;

				var decorView = MauiContext.Context.GetActivity()?.Window?.DecorView;
				Assert.NotNull(decorView);

				decorView.GetLocationOnScreen(location);
				var screenHeight = location[1] + decorView.Height;

				Assert.True(Math.Abs(screenHeight - bottomNavBottom) < 2,
					$"BottomNavigationView should extend to screen bottom. Expected bottom at {screenHeight}px, but was at {bottomNavBottom}px (gap of {screenHeight - bottomNavBottom}px)");
			});
		}

		[Theory(DisplayName = "Back-to-back PushAsync does not leak tabs (Issue 35331)")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task BackToBackPushAsyncDoesNotLeakTabs(bool bottomTabs)
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage(bottomTabs, pages: new[]
			{
				new ContentPage() { Title = "Tab 1" },
				new ContentPage() { Title = "Tab 2" }
			});

			var navPage = new NavigationPage(tabbedPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(tabbedPage.CurrentPage);

				// Push two pages back-to-back. The first push triggers
				// TabbedPage.Disappearing → RemoveTabs(). The second push
				// must not break state even though _tabLayoutFragment is
				// already null after the first RemoveTabs().
				var page1 = new ContentPage { Title = "Detail 1" };
				await navPage.PushAsync(page1);
				await OnNavigatedToAsync(page1);

				var page2 = new ContentPage { Title = "Detail 2" };
				await navPage.PushAsync(page2);
				await OnNavigatedToAsync(page2);

				// Pop back to the first detail page
				await navPage.PopAsync();
				await OnNavigatedToAsync(page1);

				// Pop back to the TabbedPage — tabs should be restored
				await navPage.PopAsync();
				await OnNavigatedToAsync(tabbedPage.CurrentPage);

				// Verify tabs are visible again
				if (bottomTabs)
				{
					var bottomNav = GetBottomNavigationView(tabbedPage.Handler as IPlatformViewHandler);
					Assert.NotNull(bottomNav);
					Assert.True(bottomNav.Visibility == global::Android.Views.ViewStates.Visible,
						"BottomNavigationView should be visible after popping back to TabbedPage");
				}
			});
		}

		BottomNavigationView GetBottomNavigationView(IPlatformViewHandler tabViewHandler)
		{
			var layout = tabViewHandler.PlatformView.FindParent((view) => view is CoordinatorLayout)
				as CoordinatorLayout;

			return layout.GetFirstChildOfType<BottomNavigationView>();
		}

		AppBarLayout GetAppBarLayout(IPlatformViewHandler tabViewHandler)
		{
			var layout = tabViewHandler.PlatformView.FindParent((view) => view is CoordinatorLayout)
				as CoordinatorLayout;

			return layout.GetFirstChildOfType<AppBarLayout>();
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

		async Task ValidateTabBarIconColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(
					GetBottomNavigationView((tabbedPage.Handler as IPlatformViewHandler)),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(
					GetBottomNavigationView((tabbedPage.Handler as IPlatformViewHandler)),
					tabText, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(
					GetBottomNavigationView((tabbedPage.Handler as IPlatformViewHandler)),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(
					GetBottomNavigationView((tabbedPage.Handler as IPlatformViewHandler)),
					tabText, iconColor, MauiContext);
			}
		}
	}
}
