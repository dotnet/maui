using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellLifeCycleTests : ShellTestBase
	{
		const string ContentRoute = "content";
		const string SectionRoute = "section";
		const string ItemRoute = "item";

		[Test]
		public void AppearingOnCreate()
		{
			Shell shell = new Shell();

			FlyoutItem flyoutItem = new FlyoutItem();
			Tab tab = new Tab();
			ShellContent content = new ShellContent() { Content = new ContentPage() };

			bool flyoutAppearing = false;
			bool tabAppearing = false;
			bool contentAppearing = false;

			flyoutItem.Appearing += (_, __) => flyoutAppearing = true;
			tab.Appearing += (_, __) => tabAppearing = true;
			content.Appearing += (_, __) => contentAppearing = true;

			shell.Items.Add(flyoutItem);
			flyoutItem.Items.Add(tab);
			tab.Items.Add(content);

			Assert.True(flyoutAppearing, "Flyout appearing");
			Assert.True(tabAppearing, "Tab Appearing");
			Assert.True(contentAppearing, "Content Appearing");
		}

		[Test]
		public void MisfiringOfAppearingWithMultipleTabs()
		{
			Shell shell = new Shell();

			var item0 = CreateShellItem(shellContentRoute: "Outbox", templated: true);
			var item1 = CreateShellItem(shellSectionRoute: "RequestType1", shellContentRoute: "RequestType1Details", templated: true);
			var section2 = CreateShellSection(shellSectionRoute: "RequestType2",  shellContentRoute: "RequestType2Dates", templated: true);

			item1.Items.Add(section2);
			shell.Items.Add(item0);
			shell.Items.Add(item1);

			int appearingCounter = 0;
			shell.GoToAsync("//Outbox");
			shell.GoToAsync("//RequestType1Details");
			shell.GoToAsync("//Outbox");


			item1.Items[0].Appearing += (_, __) =>
			{
				appearingCounter++;
			};

			shell.GoToAsync("//RequestType2Dates");
			Assert.AreEqual(0, appearingCounter);
		}


		[Test]
		public void AppearingOnCreateFromTemplate()
		{
			Shell shell = new Shell();

			FlyoutItem flyoutItem = new FlyoutItem();
			Tab tab = new Tab();
			ContentPage page = new ContentPage();
			ShellContent content = new ShellContent() { ContentTemplate = new DataTemplate(() => page) };

			bool flyoutAppearing = false;
			bool tabAppearing = false;
			bool contentAppearing = false;
			bool pageAppearing = false;

			flyoutItem.Appearing += (_, __) => flyoutAppearing = true;
			tab.Appearing += (_, __) => tabAppearing = true;
			content.Appearing += (_, __) => contentAppearing = true;
			page.Appearing += (_, __) => pageAppearing = true;

			shell.Items.Add(flyoutItem);
			flyoutItem.Items.Add(tab);
			tab.Items.Add(content);

			Assert.True(flyoutAppearing, "Flyout appearing");
			Assert.True(tabAppearing, "Tab Appearing");

			// Because the is a page template the content appearing events won't fire until the page is created
			Assert.IsFalse(contentAppearing, "Content Appearing");
			Assert.IsFalse(pageAppearing, "Page Appearing");

			var createdContent = (content as IShellContentController).GetOrCreateContent();

			Assert.AreEqual(createdContent, page);
			Assert.IsTrue(contentAppearing, "Content Appearing");
			Assert.IsTrue(pageAppearing, "Page Appearing");
		}


		[TestCase(true)]
		[TestCase(false)]
		public void EnsureOnAppearingFiresAfterParentIsSet(bool templated)
		{
			Shell shell = new Shell();

			ContentPage page = new ContentPage();

			bool parentSet = false;
			bool pageAppearing = false;
			page.Appearing += (_, __) =>
			{
				if (page.Parent == null || !parentSet)
					throw new Exception("Appearing firing before parent set is called");

				pageAppearing = true;
			};

			page.ParentSet += (_, __) => parentSet = true;
			shell.Items.Add(CreateShellItem(page, templated: templated));

			var createdContent = (shell.Items[0].Items[0].Items[0] as IShellContentController).GetOrCreateContent();

			Assert.IsTrue(pageAppearing);
		}

		[Test]
		public async Task EnsureOnAppearingFiresForNavigatedToPage()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync("LifeCyclePage");

			var page = (LifeCyclePage)shell.GetVisiblePage();

			Assert.IsTrue(page.Appearing);
			Assert.IsTrue(page.ParentSet);
		}

		[Test]
		public async Task EnsureOnAppearingFiresForLastPageOnly()
		{
			Shell shell = new Shell();
			LifeCyclePage shellContentPage = new LifeCyclePage();
			shell.Items.Add(CreateShellItem(page: shellContentPage));
			await shell.GoToAsync("LifeCyclePage/LifeCyclePage");

			var page = (LifeCyclePage)shell.GetVisiblePage();
			var nonVisiblePage = (LifeCyclePage)shell.Navigation.NavigationStack[1];

			Assert.IsFalse(nonVisiblePage.Appearing);
			Assert.IsTrue(page.Appearing);
		}
		
		[Test]
		public async Task EnsureOnAppearingFiresForLastPageOnlyAbsoluteRoute()
		{
			Shell shell = new Shell();
			LifeCyclePage shellContentPage = new LifeCyclePage();
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem(page: shellContentPage, shellItemRoute:"ShellItemRoute"));
			await shell.GoToAsync("///ShellItemRoute/LifeCyclePage/LifeCyclePage");

			var page = (LifeCyclePage)shell.GetVisiblePage();
			var nonVisiblePage = (LifeCyclePage)shell.Navigation.NavigationStack[1];

			Assert.IsFalse(shellContentPage.Appearing);
			Assert.IsFalse(nonVisiblePage.Appearing);
			Assert.IsTrue(page.Appearing);
		}


		[Test]
		public async Task EnsureOnAppearingFiresForPushedPage()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.Navigation.PushAsync(new LifeCyclePage());
			var page = (LifeCyclePage)shell.GetVisiblePage();
			Assert.IsTrue(page.Appearing);
			Assert.IsTrue(page.ParentSet);
		}

		[Test]
		public async Task NavigatedFiresAfterContentIsCreatedWhenUsingTemplate()
		{
			Shell shell = new Shell();

			FlyoutItem flyoutItem = new FlyoutItem();
			Tab tab = new Tab();
			ContentPage page = null;
			ShellContent content = new ShellContent()
			{
				Route = "destination",
				ContentTemplate = new DataTemplate(() =>
				{
					page = new ContentPage();
					return page;
				})
			};

			flyoutItem.Items.Add(tab);
			tab.Items.Add(content);

			shell.Items.Add(CreateShellItem());
			shell.Items.Add(flyoutItem);

			Assert.AreNotEqual(shell.CurrentItem.CurrentItem.CurrentItem, content);
			Assert.IsNull(page);

			bool navigated = false;
			shell.Navigated += (_, __) =>
			{
				Assert.IsNotNull(page);
				Assert.IsNotNull((content as IShellContentController).Page);
				navigated = true;
			};

			await shell.GoToAsync("///destination");

			// content hasn't been created yet
			Assert.IsFalse(navigated);

			var createPage = (content as IShellContentController).GetOrCreateContent();
			Assert.AreEqual(createPage, page);
			Assert.IsTrue(navigated);
		}

		[Test]
		public void AppearingOnShellContentChanged()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			var section = item.SearchForRoute<ShellSection>(SectionRoute);

			var content = new ShellContent();
			section.Items.Insert(0, content);
			section.CurrentItem = content;
			shell.Items.Add(item);
			ShellLifeCycleState state = new ShellLifeCycleState(shell);

			state.AllFalse();

			Assert.AreEqual(content, section.CurrentItem);

			section.CurrentItem = shell.SearchForRoute<ShellContent>(ContentRoute);

			Assert.IsTrue(state.ContentAppearing);

			section.CurrentItem = content;

			Assert.IsFalse(state.ContentAppearing);
		}


		[Test]
		public void AppearingOnShellSectionChanged()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			var section = item.SearchForRoute<ShellSection>(SectionRoute);
			var newSection = CreateShellSection();
			item.Items.Insert(0, newSection);
			item.CurrentItem = newSection;
			shell.Items.Add(item);
			ShellLifeCycleState state = new ShellLifeCycleState(shell);
			state.AllFalse();


			Assert.AreEqual(newSection, item.CurrentItem);
			Assert.AreNotEqual(section, item.CurrentItem);

			item.CurrentItem = section;

			Assert.IsTrue(state.SectionAppearing);
			Assert.IsTrue(state.ContentAppearing);

			item.CurrentItem = newSection;

			Assert.IsFalse(state.SectionAppearing);
			Assert.IsFalse(state.ContentAppearing);
		}

		[Test]
		public void AppearingOnShellItemChanged()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			var item2 = CreateShellItem();
			shell.Items.Add(item2);
			shell.Items.Add(item);
			ShellLifeCycleState state = new ShellLifeCycleState(shell);
			state.AllFalse();

			Assert.AreEqual(shell.CurrentItem, item2);
			Assert.AreNotEqual(shell.CurrentItem, item);

			shell.CurrentItem = item;

			Assert.IsTrue(state.ItemAppearing);
			Assert.IsTrue(state.SectionAppearing);
			Assert.IsTrue(state.ContentAppearing);

			shell.CurrentItem = item2;

			Assert.IsFalse(state.ItemAppearing);
			Assert.IsFalse(state.SectionAppearing);
			Assert.IsFalse(state.ContentAppearing);
		}


		[Test]
		public async Task ShellPartWithModalPush()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			lifeCycleState.AllTrue();

			ContentPage page = new ContentPage();
			await shell.Navigation.PushModalAsync(page);
			lifeCycleState.AllFalse();
			await shell.Navigation.PopModalAsync();
			lifeCycleState.AllTrue();
		}


		[Test]
		public async Task ShellPartWithPagePush()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			lifeCycleState.AllTrue();

			await shell.Navigation.PushAsync(new ContentPage());
			//if you're just pushing a page then the section and item are still visible but the content is not
			Assert.IsFalse(lifeCycleState.PageAppearing);
			Assert.IsFalse(lifeCycleState.ContentAppearing);
			Assert.IsTrue(lifeCycleState.SectionAppearing);
			Assert.IsTrue(lifeCycleState.ItemAppearing);

			await shell.Navigation.PushAsync(new ContentPage());
			Assert.IsFalse(lifeCycleState.PageAppearing);
			Assert.IsFalse(lifeCycleState.ContentAppearing);
			Assert.IsTrue(lifeCycleState.SectionAppearing);
			Assert.IsTrue(lifeCycleState.ItemAppearing);

			await shell.Navigation.PopAsync();
			Assert.IsFalse(lifeCycleState.PageAppearing);
			Assert.IsFalse(lifeCycleState.ContentAppearing);
			Assert.IsTrue(lifeCycleState.SectionAppearing);
			Assert.IsTrue(lifeCycleState.ItemAppearing);
			await shell.Navigation.PopAsync();
			lifeCycleState.AllTrue();
		}

		[Test]
		public async Task ShellPartWithPopToRoot()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			lifeCycleState.AllTrue();

			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			Assert.IsFalse(lifeCycleState.PageAppearing);
			Assert.IsFalse(lifeCycleState.ContentAppearing);
			Assert.IsTrue(lifeCycleState.SectionAppearing);
			Assert.IsTrue(lifeCycleState.ItemAppearing);

			await shell.Navigation.PopToRootAsync();
			lifeCycleState.AllTrue();
		}


		[Test]
		public async Task PagePushModal()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			ContentPage page = new ContentPage();

			bool appearing = false;

			page.Appearing += (_, __) => appearing = true;
			page.Disappearing += (_, __) => appearing = false;

			await shell.Navigation.PushModalAsync(page);
			Assert.IsTrue(appearing);
			lifeCycleState.AllFalse();

			await shell.Navigation.PopModalAsync();
			Assert.IsFalse(appearing);
			lifeCycleState.AllTrue();
		}

		[Test]
		public async Task PagePush()
		{
			Shell shell = new Shell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			shell.Items.Add(item);

			ContentPage page = new ContentPage();
			bool appearing = false;

			page.Appearing += (_, __) => appearing = true;
			page.Disappearing += (_, __) => appearing = false;

			await shell.Navigation.PushAsync(page);
			Assert.IsTrue(appearing);
			await shell.Navigation.PopAsync();
			Assert.IsFalse(appearing);
		}


		[Test]
		public void OnNavigatedOnlyFiresOnce()
		{
			int navigated = 0;
			Shell shell = new Shell();
			shell.Navigated += (_, __) =>
			{
				navigated++;
			};

			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			shell.Items.Add(item);
			Assert.AreEqual(1, navigated);
		}


		[Test]
		public void AppearingOnlyForVisiblePage()
		{
			Shell shell = new Shell();
			var pageAppearing = new ContentPage();
			var pageNotAppearing = new ContentPage();

			FlyoutItem flyoutItem = new FlyoutItem();
			Tab tab = new Tab();
			ShellContent content = new ShellContent() { Content = pageAppearing };

			bool pageAppearingFired = false;
			bool pageNotAppearingFired = false;

			pageAppearing.Appearing += (_, __) => pageAppearingFired = true;
			pageNotAppearing.Appearing += (_, __) =>
			{
				pageNotAppearingFired = true;
			};

			shell.Items.Add(flyoutItem);
			flyoutItem.Items.Add(tab);
			tab.Items.Add(content);

			var notAppearingContent = new ShellContent();
			tab.Items.Add(notAppearingContent);
			notAppearingContent.Content = pageNotAppearing;

			Assert.True(pageAppearingFired, "Correct Page Appearing Fired");
			Assert.False(pageNotAppearingFired, "Incorrect Page Appearing Fired");
		}

		public class LifeCyclePage : ContentPage
		{
			public bool Appearing;
			public bool ParentSet;

			protected override void OnAppearing()
			{
				base.OnAppearing();
				Appearing = true;
			}

			protected override void OnParentSet()
			{
				base.OnParentSet();
				ParentSet = true;
			}
		}

		public override void Setup()
		{
			base.Setup();
			Routing.RegisterRoute("LifeCyclePage", typeof(LifeCyclePage));
		}

		class ShellLifeCycleState
		{
			public bool ItemAppearing;
			public bool SectionAppearing;
			public bool ContentAppearing;
			public bool PageAppearing;

			public ShellLifeCycleState(Shell shell)
			{
				var shellContent = shell.SearchForRoute<ShellContent>(ContentRoute);
				var contentPage = (shellContent as IShellContentController).GetOrCreateContent();

				shell.SearchForRoute(ItemRoute).Appearing += (_, __) => ItemAppearing = true;
				shell.SearchForRoute(SectionRoute).Appearing += (_, __) => SectionAppearing = true;
				shell.SearchForRoute(ContentRoute).Appearing += (_, __) => ContentAppearing = true;
				shellContent.Appearing += (_, __) => ContentAppearing = true;
				contentPage.Appearing += (_, __) => PageAppearing = true;

				shell.SearchForRoute(ItemRoute).Disappearing += (_, __) => ItemAppearing = false;
				shell.SearchForRoute(SectionRoute).Disappearing += (_, __) => SectionAppearing = false;
				shellContent.Disappearing += (_, __) => ContentAppearing = false;
				contentPage.Disappearing += (_, __) => PageAppearing = false;
			}
			public ShellLifeCycleState(BaseShellItem baseShellItem)
			{
				var shellContent = baseShellItem.SearchForRoute<ShellContent>(ContentRoute);
				var contentPage = (shellContent as IShellContentController).GetOrCreateContent();

				baseShellItem.SearchForRoute(ItemRoute).Appearing += (_, __) => ItemAppearing = true;
				baseShellItem.SearchForRoute(SectionRoute).Appearing += (_, __) => SectionAppearing = true;
				shellContent.Appearing += (_, __) => ContentAppearing = true;
				contentPage.Appearing += (_, __) => PageAppearing = true;

				baseShellItem.SearchForRoute(ItemRoute).Disappearing += (_, __) => ItemAppearing = false;
				baseShellItem.SearchForRoute(SectionRoute).Disappearing += (_, __) => SectionAppearing = false;
				shellContent.Disappearing += (_, __) => ContentAppearing = false;
				contentPage.Disappearing += (_, __) => PageAppearing = false;
			}

			public void AllFalse()
			{
				Assert.IsFalse(ItemAppearing);
				Assert.IsFalse(SectionAppearing);
				Assert.IsFalse(ContentAppearing);
				Assert.IsFalse(PageAppearing);
			}
			public void AllTrue()
			{
				Assert.IsTrue(ItemAppearing);
				Assert.IsTrue(SectionAppearing);
				Assert.IsTrue(ContentAppearing);
				Assert.IsTrue(PageAppearing);
			}
		}
	}
}
