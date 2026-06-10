using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests
	{
		UITabBar GetTabBar(TabbedPage tabbedPage)
		{
			var pagerParent = (tabbedPage.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			return pagerParent.Subviews.FirstOrDefault(v => v.GetType() == typeof(UITabBar)) as UITabBar;
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
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(
					GetTabBar(tabbedPage),
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
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
		}

		[Theory("Handler: Tab switch fires Appearing/Disappearing on NavigationPage content")]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task Handler_TabSwitchFiresAppearingDisappearing(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
				});
			});

			var page1Content = new ContentPage { Title = "Tab1 Content", Content = new Label { Text = "Tab 1" } };
			var page2Content = new ContentPage { Title = "Tab2 Content", Content = new Label { Text = "Tab 2" } };
			var navPage1 = new NavigationPage(page1Content) { Title = "Tab 1" };
			var navPage2 = new NavigationPage(page2Content) { Title = "Tab 2" };

			bool page1Appeared = false;
			bool page1Disappeared = false;
			bool page2Appeared = false;
			bool page2Disappeared = false;

			page1Content.Appearing += (_, _) => page1Appeared = true;
			page1Content.Disappearing += (_, _) => page1Disappeared = true;
			page2Content.Appearing += (_, _) => page2Appeared = true;
			page2Content.Disappearing += (_, _) => page2Disappeared = true;

			var tabbedPage = CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled, new Page[] { navPage1, navPage2 });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				// Tab 1 is initially selected — its content should have appeared
				await OnNavigatedToAsync(page1Content);
				Assert.True(page1Appeared, "Tab1 content should have appeared on initial load");

				// Switch to Tab 2
				page1Disappeared = false;
				page2Appeared = false;
				tabbedPage.CurrentPage = navPage2;
				await OnNavigatedToAsync(page2Content);
				await Task.Delay(200); // Allow ViewDidAppear/ViewDidDisappear to fire

				Assert.True(page1Disappeared, "Tab1 content should have disappeared after switching to Tab2");
				Assert.True(page2Appeared, "Tab2 content should have appeared after switching to Tab2");

				// Switch back to Tab 1
				page1Appeared = false;
				page2Disappeared = false;
				tabbedPage.CurrentPage = navPage1;
				await OnNavigatedToAsync(page1Content);
				await Task.Delay(200);

				Assert.True(page2Disappeared, "Tab2 content should have disappeared after switching back to Tab1");
				Assert.True(page1Appeared, "Tab1 content should have appeared after switching back to Tab1");
			});
		}
	}
}
