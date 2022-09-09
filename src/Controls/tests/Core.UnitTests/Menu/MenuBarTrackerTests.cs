using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.MenuBarTests
{
	public class MenuBarTrackerTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var tracker = new MenuBarTracker();
			Assert.Null(tracker.Target);
			Assert.False(tracker.ToolbarItems.Any());
		}

		[Fact]
		public void SimpleTrackEmpty()
		{
			var tracker = new MenuBarTracker();

			var page = new ContentPage();
			tracker.Target = page;

			Assert.False(tracker.ToolbarItems.Any());
		}

		[Fact]
		public void SimpleTrackWithItems()
		{
			var tracker = new MenuBarTracker();

			ToolbarItem item1, item2;
			var page = new ContentPage
			{
				MenuBarItems = {
					new MenuBarItem (),
					new MenuBarItem ()
				}
			};
			tracker.Target = page;

			Assert.True(tracker.ToolbarItems.Contains(page.MenuBarItems[0]));
			Assert.True(tracker.ToolbarItems.Contains(page.MenuBarItems[1]));
		}

		[Fact]
		public void TrackPreConstructedTabbedPage()
		{
			var tracker = new MenuBarTracker();

			var menubaritem1 = new MenuBarItem();
			var menubarItem2 = new MenuBarItem();
			var menubarItem3 = new MenuBarItem();

			var subPage1 = new ContentPage
			{
				MenuBarItems = { menubaritem1 }
			};

			var subPage2 = new ContentPage
			{
				MenuBarItems = { menubarItem2, menubarItem3 }
			};

			var tabbedpage = new TabbedPage
			{
				Children = {
					subPage1,
					subPage2
				}
			};

			tabbedpage.CurrentPage = subPage1;

			tracker.Target = tabbedpage;

			Assert.True(tracker.ToolbarItems.Count() == 1);
			Assert.True(tracker.ToolbarItems.First() == subPage1.MenuBarItems[0]);

			bool changed = false;
			tracker.CollectionChanged += (sender, args) => changed = true;

			tabbedpage.CurrentPage = subPage2;

			Assert.True(tracker.ToolbarItems.Count() == 2);
			Assert.True(tracker.ToolbarItems.First() == subPage2.MenuBarItems[0]);
			Assert.True(tracker.ToolbarItems.Last() == subPage2.MenuBarItems[1]);
			Assert.True(changed);
		}

		[Fact]
		public void AdditionalTargets()
		{
			var tracker = new MenuBarTracker();

			var menubaritem1 = new MenuBarItem();
			var menubarItem2 = new MenuBarItem();

			var page = new ContentPage
			{
				MenuBarItems = {
					menubaritem1
				}
			};

			var additionalPage = new ContentPage
			{
				MenuBarItems = { menubarItem2 }
			};

			tracker.Target = page;
			tracker.AdditionalTargets = new[] { additionalPage };

			Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
			Assert.True(tracker.ToolbarItems.Contains(menubarItem2));
		}

		[Fact]
		public async Task PushAfterTrackingStarted()
		{
			var tracker = new MenuBarTracker();

			var menubaritem1 = new MenuBarItem();
			var menubarItem2 = new MenuBarItem();

			var page = new NavigationPage
			{
				MenuBarItems = {
					menubaritem1
				}
			};

			var firstPage = new ContentPage
			{
				MenuBarItems = { menubarItem2 }
			};

			tracker.Target = page;

			Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
			Assert.False(tracker.ToolbarItems.Contains(menubarItem2));

			await page.Navigation.PushAsync(firstPage);

			Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
			Assert.True(tracker.ToolbarItems.Contains(menubarItem2));
		}

		[Fact]
		public async Task PopAfterTrackingStarted()
		{
			var tracker = new MenuBarTracker();

			var menubaritem1 = new MenuBarItem();
			var menubarItem2 = new MenuBarItem();

			var page = new NavigationPage(new ContentPage())
			{
				MenuBarItems = {
					menubaritem1
				}
			};

			var firstPage = new ContentPage
			{
				MenuBarItems = { menubarItem2 }
			};

			tracker.Target = page;

			await page.Navigation.PushAsync(firstPage);

			Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
			Assert.True(tracker.ToolbarItems.Contains(menubarItem2));

			await page.Navigation.PopAsync();

			Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
			Assert.False(tracker.ToolbarItems.Contains(menubarItem2));
		}

		[Fact]
		public void UnsetTarget()
		{
			var tracker = new MenuBarTracker();

			MenuBarItem item1, item2;
			var page = new ContentPage
			{
				MenuBarItems = {
					new MenuBarItem (),
					new MenuBarItem ()
				}
			};
			tracker.Target = page;

			Assert.True(tracker.ToolbarItems.Count() == 2);

			tracker.Target = null;

			Assert.False(tracker.ToolbarItems.Any());
		}

		[Fact]
		public void AddingMenuBarItemsFireCollectionChanged()
		{
			var tracker = new MenuBarTracker();

			var menubaritem1 = new MenuBarItem();
			var menubarItem2 = new MenuBarItem();

			var subPage1 = new ContentPage
			{
				MenuBarItems = { menubaritem1 }
			};

			tracker.Target = new NavigationPage(subPage1);

			bool changed = false;
			tracker.CollectionChanged += (sender, args) => changed = true;

			subPage1.MenuBarItems.Add(menubarItem2);
			Assert.True(changed);
		}
	}
}
