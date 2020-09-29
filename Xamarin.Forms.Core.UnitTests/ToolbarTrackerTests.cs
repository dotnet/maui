using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	internal class ToolbarTrackerTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var tracker = new ToolbarTracker();
			Assert.Null(tracker.Target);
			Assert.False(tracker.ToolbarItems.Any());
		}

		[Test]
		public void SimpleTrackEmpty()
		{
			var tracker = new ToolbarTracker();

			var page = new ContentPage();
			tracker.Target = page;

			Assert.False(tracker.ToolbarItems.Any());
		}

		[Test]
		public void SimpleTrackWithItems()
		{
			var tracker = new ToolbarTracker();

			ToolbarItem item1, item2;
			var page = new ContentPage
			{
				ToolbarItems = {
					new ToolbarItem ("Foo", "Foo.png", () => {}),
					new ToolbarItem ("Bar", "Bar.png", () => {})
				}
			};
			tracker.Target = page;

			Assert.True(tracker.ToolbarItems.Contains(page.ToolbarItems[0]));
			Assert.True(tracker.ToolbarItems.Contains(page.ToolbarItems[1]));
		}

		[Test]
		public void TrackPreConstructedTabbedPage()
		{
			var tracker = new ToolbarTracker();

			var toolbarItem1 = new ToolbarItem("Foo", "Foo.png", () => { });
			var toolbarItem2 = new ToolbarItem("Foo", "Foo.png", () => { });
			var toolbarItem3 = new ToolbarItem("Foo", "Foo.png", () => { });

			var subPage1 = new ContentPage
			{
				ToolbarItems = { toolbarItem1 }
			};

			var subPage2 = new ContentPage
			{
				ToolbarItems = { toolbarItem2, toolbarItem3 }
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
			Assert.True(tracker.ToolbarItems.First() == subPage1.ToolbarItems[0]);

			bool changed = false;
			tracker.CollectionChanged += (sender, args) => changed = true;

			tabbedpage.CurrentPage = subPage2;

			Assert.True(tracker.ToolbarItems.Count() == 2);
			Assert.True(tracker.ToolbarItems.First() == subPage2.ToolbarItems[0]);
			Assert.True(tracker.ToolbarItems.Last() == subPage2.ToolbarItems[1]);
		}

		[Test]
		public void AdditionalTargets()
		{
			var tracker = new ToolbarTracker();

			var toolbarItem1 = new ToolbarItem("Foo", "Foo.png", () => { });
			var toolbarItem2 = new ToolbarItem("Foo", "Foo.png", () => { });

			var page = new ContentPage
			{
				ToolbarItems = {
					toolbarItem1
				}
			};

			var additionalPage = new ContentPage
			{
				ToolbarItems = { toolbarItem2 }
			};

			tracker.Target = page;
			tracker.AdditionalTargets = new[] { additionalPage };

			Assert.True(tracker.ToolbarItems.Contains(toolbarItem1));
			Assert.True(tracker.ToolbarItems.Contains(toolbarItem2));
		}

		[Test]
		public async Task PushAfterTrackingStarted()
		{
			var tracker = new ToolbarTracker();

			var toolbarItem1 = new ToolbarItem("Foo", "Foo.png", () => { });
			var toolbarItem2 = new ToolbarItem("Foo", "Foo.png", () => { });

			var page = new NavigationPage
			{
				ToolbarItems = {
					toolbarItem1
				}
			};

			var firstPage = new ContentPage
			{
				ToolbarItems = { toolbarItem2 }
			};

			tracker.Target = page;

			Assert.True(tracker.ToolbarItems.Contains(toolbarItem1));
			Assert.False(tracker.ToolbarItems.Contains(toolbarItem2));

			await page.Navigation.PushAsync(firstPage);

			Assert.True(tracker.ToolbarItems.Contains(toolbarItem1));
			Assert.True(tracker.ToolbarItems.Contains(toolbarItem2));
		}

		[Test]
		public async Task PopAfterTrackingStarted()
		{
			var tracker = new ToolbarTracker();

			var toolbarItem1 = new ToolbarItem("Foo", "Foo.png", () => { });
			var toolbarItem2 = new ToolbarItem("Foo", "Foo.png", () => { });

			var page = new NavigationPage(new ContentPage())
			{
				ToolbarItems = {
					toolbarItem1
				}
			};

			var firstPage = new ContentPage
			{
				ToolbarItems = { toolbarItem2 }
			};

			tracker.Target = page;

			await page.Navigation.PushAsync(firstPage);

			Assert.True(tracker.ToolbarItems.Contains(toolbarItem1));
			Assert.True(tracker.ToolbarItems.Contains(toolbarItem2));

			await page.Navigation.PopAsync();

			Assert.True(tracker.ToolbarItems.Contains(toolbarItem1));
			Assert.False(tracker.ToolbarItems.Contains(toolbarItem2));
		}

		[Test]
		public void UnsetTarget()
		{
			var tracker = new ToolbarTracker();

			ToolbarItem item1, item2;
			var page = new ContentPage
			{
				ToolbarItems = {
					new ToolbarItem ("Foo", "Foo.png", () => {}),
					new ToolbarItem ("Bar", "Bar.png", () => {})
				}
			};
			tracker.Target = page;

			Assert.True(tracker.ToolbarItems.Count() == 2);

			tracker.Target = null;

			Assert.False(tracker.ToolbarItems.Any());
		}
	}
}