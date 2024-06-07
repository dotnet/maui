using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests : ControlsHandlerTestBase
	{
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

				menuItem1.Icon.AssertColorAtCenter(Android.Graphics.Color.Blue);
				menuItem2.Icon.AssertColorAtCenter(Android.Graphics.Color.Blue);

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

		BottomNavigationView GetBottomNavigationView(IPlatformViewHandler tabViewHandler)
		{
			var layout = tabViewHandler.PlatformView.FindParent((view) => view is CoordinatorLayout)
				as CoordinatorLayout;

			return layout.GetFirstChildOfType<BottomNavigationView>();
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
