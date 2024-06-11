using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellLifeCycleTests : ShellTestBase
	{
		const string ContentRoute = "content";
		const string SectionRoute = "section";
		const string ItemRoute = "item";

		[Fact]
		public void AppearingOnCreate()
		{
			Shell shell = new TestShell();

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

		[Fact]
		public void MisfiringOfAppearingWithMultipleTabs()
		{
			Shell shell = new TestShell();

			var item0 = CreateShellItem(shellContentRoute: "Outbox", templated: true);
			var item1 = CreateShellItem(shellSectionRoute: "RequestType1", shellContentRoute: "RequestType1Details", templated: true);
			var section2 = CreateShellSection(shellSectionRoute: "RequestType2", shellContentRoute: "RequestType2Dates", templated: true);

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
			Assert.Equal(0, appearingCounter);
		}


		[Fact]
		public void AppearingOnCreateFromTemplate()
		{
			Shell shell = new TestShell();

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
			Assert.False(contentAppearing, "Content Appearing");
			Assert.False(pageAppearing, "Page Appearing");

			var createdContent = (content as IShellContentController).GetOrCreateContent();

			Assert.Equal(createdContent, page);
			Assert.True(contentAppearing, "Content Appearing");
			Assert.True(pageAppearing, "Page Appearing");
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void EnsureOnAppearingFiresAfterParentIsSet(bool templated)
		{
			Shell shell = new TestShell();

			ContentPage page = new ContentPage();

			bool parentSet = false;
			bool pageAppearing = false;
			page.Appearing += (_, __) =>
			{
				if (page.Parent == null || !parentSet)
				{
					throw new Exception("Appearing firing before parent set is called");
				}

				pageAppearing = true;
			};

			page.ParentSet += (_, __) => parentSet = true;
			shell.Items.Add(CreateShellItem(page, templated: templated));

			var createdContent = (shell.Items[0].Items[0].Items[0] as IShellContentController).GetOrCreateContent();

			Assert.True(pageAppearing);
		}

		[Fact]
		public async Task EnsureOnAppearingFiresForNavigatedToPage()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync("LifeCyclePage");

			var page = (LifeCyclePage)shell.GetVisiblePage();

			Assert.True(page.Appearing);
			Assert.True(page.ParentSet);
		}

		[Fact]
		public async Task EnsureOnAppearingFiresForLastPageOnly()
		{
			Shell shell = new TestShell();
			LifeCyclePage shellContentPage = new LifeCyclePage();
			shell.Items.Add(CreateShellItem(page: shellContentPage));
			await shell.GoToAsync("LifeCyclePage/LifeCyclePage");

			var page = (LifeCyclePage)shell.GetVisiblePage();
			var nonVisiblePage = (LifeCyclePage)shell.Navigation.NavigationStack[1];

			Assert.False(nonVisiblePage.Appearing);
			Assert.True(page.Appearing);
		}

		[Fact]
		public async Task EnsureOnAppearingFiresForLastPageOnlyAbsoluteRoute()
		{
			Shell shell = new TestShell();
			LifeCyclePage shellContentPage = new LifeCyclePage();
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem(page: shellContentPage, shellItemRoute: "ShellItemRoute"));
			await shell.GoToAsync("///ShellItemRoute/LifeCyclePage/LifeCyclePage");

			var page = (LifeCyclePage)shell.GetVisiblePage();
			var nonVisiblePage = (LifeCyclePage)shell.Navigation.NavigationStack[1];

			Assert.False(shellContentPage.Appearing);
			Assert.False(nonVisiblePage.Appearing);
			Assert.True(page.Appearing);
		}


		[Fact]
		public async Task EnsureOnAppearingFiresForPushedPage()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());
			await shell.Navigation.PushAsync(new LifeCyclePage());
			var page = (LifeCyclePage)shell.GetVisiblePage();
			Assert.True(page.Appearing);
			Assert.True(page.ParentSet);
		}

		[Fact]
		public async Task NavigatedFiresAfterContentIsCreatedWhenUsingTemplate()
		{
			Shell shell = new TestShell();

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

			Assert.NotEqual(shell.CurrentItem.CurrentItem.CurrentItem, content);
			Assert.Null(page);

			bool navigated = false;
			shell.Navigated += (_, __) =>
			{
				Assert.NotNull(page);
				Assert.NotNull((content as IShellContentController).Page);
				navigated = true;
			};

			await shell.GoToAsync("///destination");

			// content hasn't been created yet
			Assert.False(navigated);

			var createPage = (content as IShellContentController).GetOrCreateContent();
			Assert.Equal(createPage, page);
			Assert.True(navigated);
		}

		[Fact]
		public void AppearingOnShellContentChanged()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			var section = item.SearchForRoute<ShellSection>(SectionRoute);

			var content = new ShellContent();
			section.Items.Insert(0, content);
			section.CurrentItem = content;
			shell.Items.Add(item);
			ShellLifeCycleState state = new ShellLifeCycleState(shell);

			state.AllFalse();

			Assert.Equal(content, section.CurrentItem);

			section.CurrentItem = shell.SearchForRoute<ShellContent>(ContentRoute);

			Assert.True(state.ContentAppearing);

			section.CurrentItem = content;

			Assert.False(state.ContentAppearing);
		}


		[Fact]
		public void AppearingOnShellSectionChanged()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			var section = item.SearchForRoute<ShellSection>(SectionRoute);
			var newSection = CreateShellSection();
			item.Items.Insert(0, newSection);
			item.CurrentItem = newSection;
			shell.Items.Add(item);
			ShellLifeCycleState state = new ShellLifeCycleState(shell);
			state.AllFalse();


			Assert.Equal(newSection, item.CurrentItem);
			Assert.NotEqual(section, item.CurrentItem);

			item.CurrentItem = section;

			Assert.True(state.SectionAppearing);
			Assert.True(state.ContentAppearing);

			item.CurrentItem = newSection;

			Assert.False(state.SectionAppearing);
			Assert.False(state.ContentAppearing);
		}

		[Fact]
		public void AppearingOnShellItemChanged()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			var item2 = CreateShellItem();
			shell.Items.Add(item2);
			shell.Items.Add(item);
			ShellLifeCycleState state = new ShellLifeCycleState(shell);
			state.AllFalse();

			Assert.Equal(shell.CurrentItem, item2);
			Assert.NotEqual(shell.CurrentItem, item);

			shell.CurrentItem = item;

			Assert.True(state.ItemAppearing);
			Assert.True(state.SectionAppearing);
			Assert.True(state.ContentAppearing);

			shell.CurrentItem = item2;

			Assert.False(state.ItemAppearing);
			Assert.False(state.SectionAppearing);
			Assert.False(state.ContentAppearing);
		}

		[Fact]
		public async Task ShellPartWithModalPush()
		{
			Shell shell = new TestShell();
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

		[Fact]
		public async Task ShellPartWithPagePush()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			lifeCycleState.AllTrue();

			await shell.Navigation.PushAsync(new ContentPage());

			//if you're just pushing a page then the section and item are still visible but the content is not
			Assert.False(lifeCycleState.ShellContentPageAppearing);
			Assert.False(lifeCycleState.ContentAppearing);
			Assert.True(lifeCycleState.SectionAppearing);
			Assert.True(lifeCycleState.ItemAppearing);

			await shell.Navigation.PushAsync(new ContentPage());
			Assert.False(lifeCycleState.ShellContentPageAppearing);
			Assert.False(lifeCycleState.ContentAppearing);
			Assert.True(lifeCycleState.SectionAppearing);
			Assert.True(lifeCycleState.ItemAppearing);

			await shell.Navigation.PopAsync();
			Assert.False(lifeCycleState.ShellContentPageAppearing);
			Assert.False(lifeCycleState.ContentAppearing);
			Assert.True(lifeCycleState.SectionAppearing);
			Assert.True(lifeCycleState.ItemAppearing);
			await shell.Navigation.PopAsync();
			lifeCycleState.AllTrue();
		}

		[Fact]
		public async Task ShellPartWithPopToRoot()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			lifeCycleState.AllTrue();

			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			Assert.False(lifeCycleState.ShellContentPageAppearing);
			Assert.False(lifeCycleState.ContentAppearing);
			Assert.True(lifeCycleState.SectionAppearing);
			Assert.True(lifeCycleState.ItemAppearing);

			await shell.Navigation.PopToRootAsync();
			lifeCycleState.AllTrue();
		}


		[Fact]
		public async Task PagePushModal()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			ShellLifeCycleState lifeCycleState = new ShellLifeCycleState(item);
			shell.Items.Add(item);

			ContentPage page = new ContentPage();

			bool appearing = false;

			page.Appearing += (_, __) => appearing = true;
			page.Disappearing += (_, __) => appearing = false;

			await shell.Navigation.PushModalAsync(page);
			Assert.True(appearing);
			lifeCycleState.AllFalse();

			await shell.Navigation.PopModalAsync();
			Assert.False(appearing);
			lifeCycleState.AllTrue();
		}

		[Fact]
		public async Task PagePush()
		{
			Shell shell = new TestShell();
			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			shell.Items.Add(item);

			ContentPage page = new ContentPage();
			bool appearing = false;

			page.Appearing += (_, __) => appearing = true;
			page.Disappearing += (_, __) => appearing = false;

			await shell.Navigation.PushAsync(page);
			Assert.True(appearing);
			await shell.Navigation.PopAsync();
			Assert.False(appearing);
		}


		[Fact]
		public void OnNavigatedOnlyFiresOnce()
		{
			int navigated = 0;
			Shell shell = new TestShell();
			shell.Navigated += (_, __) =>
			{
				navigated++;
			};

			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			shell.Items.Add(item);
			Assert.Equal(1, navigated);
		}


		[Fact]
		public void AppearingOnlyForVisiblePage()
		{
			Shell shell = new TestShell();
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

		[Fact]
		public void OnNavigatedCalledOnce()
		{
			List<ShellNavigatedEventArgs> args = new List<ShellNavigatedEventArgs>();
			Action<ShellNavigatedEventArgs> onNavigated = (a) =>
			{
				args.Add(a);
			};

			TestShell testShell = new TestShell()
			{
				OnNavigatedHandler = onNavigated
			};

			testShell.Items.Add(base.CreateShellItem());

			Assert.Single(args);
		}


		[Fact]
		public async Task OnNavigatedFiresWhenPopping()
		{
			Routing.RegisterRoute("AlarmPage", typeof(LifeCyclePage));
			Routing.RegisterRoute("SoundsPage", typeof(LifeCyclePage));
			TestShell shell = new TestShell();

			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			shell.Items.Add(item);

			await shell.GoToAsync("AlarmPage/SoundsPage");
			shell.Reset();

			await shell.Navigation.PopAsync();
			shell.TestCount(1);
		}

		[Fact]
		public async Task OnNavigatedFiresWhenPopToRoot()
		{
			Routing.RegisterRoute("AlarmPage", typeof(LifeCyclePage));
			Routing.RegisterRoute("SoundsPage", typeof(LifeCyclePage));
			TestShell shell = new TestShell();

			var item = CreateShellItem(shellContentRoute: ContentRoute, shellSectionRoute: SectionRoute, shellItemRoute: ItemRoute);
			shell.Items.Add(item);

			await shell.GoToAsync("AlarmPage/SoundsPage");
			shell.Reset();

			await shell.Navigation.PopToRootAsync();
			shell.TestCount(1);
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

		public ShellLifeCycleTests()
		{

			Routing.RegisterRoute("LifeCyclePage", typeof(LifeCyclePage));
		}

		public class ShellLifeCycleState
		{
			public bool ItemAppearing
			{
				get;
				set;
			}

			public bool SectionAppearing
			{
				get;
				set;
			}

			public bool ContentAppearing
			{
				get;
				set;
			}

			public bool ShellContentPageAppearing
			{
				get;
				set;
			}

			public ShellLifeCycleState(Shell shell)
			{
				var shellContent = shell.SearchForRoute<ShellContent>(ContentRoute);
				var contentPage = (shellContent as IShellContentController).GetOrCreateContent();

				shell.SearchForRoute(ItemRoute).Appearing += (_, __) => ItemAppearing = true;
				shell.SearchForRoute(SectionRoute).Appearing += (_, __) => SectionAppearing = true;
				shell.SearchForRoute(ContentRoute).Appearing += (_, __) => ContentAppearing = true;
				shellContent.Appearing += (_, __) => ContentAppearing = true;
				contentPage.Appearing += (_, __) => ShellContentPageAppearing = true;

				shell.SearchForRoute(ItemRoute).Disappearing += (_, __) => ItemAppearing = false;
				shell.SearchForRoute(SectionRoute).Disappearing += (_, __) => SectionAppearing = false;
				shellContent.Disappearing += (_, __) => ContentAppearing = false;
				contentPage.Disappearing += (_, __) => ShellContentPageAppearing = false;
			}

			public ShellLifeCycleState(BaseShellItem baseShellItem)
			{
				var shellContent = baseShellItem.SearchForRoute<ShellContent>(ContentRoute);
				var contentPage = (shellContent as IShellContentController).GetOrCreateContent();

				baseShellItem.SearchForRoute(ItemRoute).Appearing += (_, __) => ItemAppearing = true;
				baseShellItem.SearchForRoute(SectionRoute).Appearing += (_, __) => SectionAppearing = true;
				shellContent.Appearing += (_, __) => ContentAppearing = true;
				contentPage.Appearing += (_, __) => ShellContentPageAppearing = true;

				baseShellItem.SearchForRoute(ItemRoute).Disappearing += (_, __) => ItemAppearing = false;
				baseShellItem.SearchForRoute(SectionRoute).Disappearing += (_, __) => SectionAppearing = false;
				shellContent.Disappearing += (_, __) => ContentAppearing = false;
				contentPage.Disappearing += (_, __) => ShellContentPageAppearing = false;
			}

			public void AllFalse()
			{
				Assert.False(ItemAppearing);
				Assert.False(SectionAppearing);
				Assert.False(ContentAppearing);
				Assert.False(ShellContentPageAppearing);
			}

			public void AllTrue()
			{
				Assert.True(ItemAppearing);
				Assert.True(SectionAppearing);
				Assert.True(ContentAppearing);
				Assert.True(ShellContentPageAppearing);
			}
		}
	}
}
