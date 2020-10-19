using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellNavigatingTests : ShellTestBase
	{

		[Test]
		public void CancelNavigation()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabtwo = MakeSimpleShellSection("tabtwo", "content");
			var tabthree = MakeSimpleShellSection("tabthree", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content");

			one.Items.Add(tabone);
			one.Items.Add(tabtwo);

			two.Items.Add(tabthree);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//one/tabone/content"));

			shell.Navigating += (s, e) =>
			{
				e.Cancel();
			};

			shell.GoToAsync(new ShellNavigationState("//two/tabfour/"));

			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//one/tabone/content"));
		}

		[TestCase("PopToRoot")]
		[TestCase("Pop")]
		public async Task DeferPopNavigation(string testCase)
		{
			TestShell shell = new TestShell()
			{
				Items = { CreateShellItem<FlyoutItem>() }
			};

			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());

			ShellNavigatingDeferral _token = null;
			shell.Navigating += async (_, args) =>
			{
				_token = args.GetDeferral();
				await Task.Delay(500);
				Assert.AreEqual(3, shell.Navigation.NavigationStack.Count);
				_token.Complete();
			};

			var source = new TaskCompletionSource<object>();
			shell.Navigated += (_, args) =>
			{
				source.SetResult(true);
			};

			if (testCase == "Pop")
			{
				await shell.Navigation.PopAsync();
				await source.Task;
				Assert.AreEqual(2, shell.Navigation.NavigationStack.Count);
			}
			else
			{
				await shell.Navigation.PopToRootAsync();
				await source.Task;
				Assert.AreEqual(1, shell.Navigation.NavigationStack.Count);
			}
		}

		[TestCase("PopToRoot")]
		[TestCase("Pop")]
		[TestCase("GoToAsync")]
		[TestCase("Push")]
		public async Task NavigationTaskCompletesAfterDeferalHasFinished(string testCase)
		{
			Routing.RegisterRoute(nameof(NavigationTaskCompletesAfterDeferalHasFinished), typeof(ContentPage));
			var shell = new TestShell()
			{
				Items = { CreateShellItem<FlyoutItem>() }
			};

			ShellNavigatingDeferral _token = null;
			shell.Navigating += async (_, args) =>
			{
				_token = args.GetDeferral();
				await Task.Delay(500);
				_token.Complete();
			};

			switch (testCase)
			{
				case "PopToRoot":
					await shell.Navigation.PopToRootAsync();
					break;
				case "Pop":
					await shell.Navigation.PopAsync();
					break;
				case "GoToAsync":
					await shell.GoToAsync(nameof(NavigationTaskCompletesAfterDeferalHasFinished));
					break;
				case "Push":
					await shell.Navigation.PushAsync(new ContentPage());
					break;
			}

			Assert.IsTrue(_token.IsCompleted);
		}

		[Test]
		public void CompletingTheSameDeferalTokenTwiceDoesntDoAnything()
		{
			var args = CreateShellNavigatedEventArgs();
			var token = args.GetDeferral();
			args.GetDeferral();

			Assert.AreEqual(2, args.DeferralCount);
			token.Complete();
			Assert.AreEqual(1, args.DeferralCount);
			token.Complete();
			Assert.AreEqual(1, args.DeferralCount);
		}

		[Test]
		public async Task DotDotNavigateBackFromPagesWithDefaultRoute()
		{
			var flyoutItem = CreateShellItem<FlyoutItem>();
			var itemRoute = Routing.GetRoute(flyoutItem.CurrentItem.CurrentItem);
			var page1 = new ContentPage();
			var page2 = new ContentPage();
			TestShell shell = new TestShell()
			{
				Items = { flyoutItem }
			};

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}"));

			await shell.Navigation.PushAsync(page1);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));

			await shell.Navigation.PushAsync(page2);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}/{Routing.GetRoute(page2)}"));

			await shell.GoToAsync("..");

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));
		}

		[Test]
		public async Task NavigationPushAndPopBasic()
		{
			var flyoutItem =
				CreateShellItem<FlyoutItem>(
					 shellItemRoute: "item",
					 shellSectionRoute: "section",
					 shellContentRoute: "content"
					);

			var itemRoute = "item/section/content";
			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var shell = new TestShell(flyoutItem);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}"));

			await shell.Navigation.PushAsync(page1);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));

			await shell.Navigation.PushAsync(page2);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}/{Routing.GetRoute(page2)}"));

			await shell.Navigation.PopAsync();

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));
		}

		[Test]
		public async Task NavigateToDefaultShellContent()
		{
			TestShell testShell = new TestShell(CreateShellItem<FlyoutItem>());
			var page = new ContentPage();

			var contentRoute = testShell.CurrentItem.CurrentItem.CurrentItem.Route;
			var pageRoute = Routing.GetRoute(page);

			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PushAsync(page);

			await testShell.GoToAsync($"//{contentRoute}/{pageRoute}");


			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{contentRoute}/{pageRoute}"));
		}

		[Test]
		public async Task PopToRootWithMultipleFlyoutItems()
		{
			TestShell testShell = new TestShell(
				CreateShellItem<FlyoutItem>(shellItemRoute: "store", shellContentRoute: "home"),
				CreateShellItem<FlyoutItem>(shellItemRoute: "second", shellContentRoute: "home")
			);

			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PopToRootAsync();
		}

		[Test]
		public async Task MultiplePopsRemoveMiddlePagesBeforeFinalPop()
		{
			TestShell testShell = new TestShell(
				CreateShellSection<NavigationMonitoringTab>(shellContentRoute: "rootpage")
			);

			var pageLeftOnStack = new ContentPage();
			var tab = (NavigationMonitoringTab)testShell.CurrentItem.CurrentItem;
			await testShell.Navigation.PushAsync(pageLeftOnStack);
			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PushAsync(new ContentPage());
			tab.NavigationsFired.Clear();
			await testShell.GoToAsync("../..");
			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//rootpage/{Routing.GetRoute(pageLeftOnStack)}"));

			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[0]);
			Assert.AreEqual("OnPopAsync", tab.NavigationsFired[1]);
			Assert.AreEqual(2, tab.NavigationsFired.Count);
		}

		[Test]
		public async Task PopToRootRemovesMiddlePagesBeforePoppingVisibleModalPages()
		{
			Routing.RegisterRoute("ModalTestPage", typeof(ShellModalTests.ModalTestPage));
			TestShell testShell = new TestShell(
				CreateShellSection<NavigationMonitoringTab>(shellContentRoute: "rootpage")
			);

			var middlePage = new ContentPage();
			var tab = (NavigationMonitoringTab)testShell.CurrentItem.CurrentItem;
			await testShell.Navigation.PushAsync(middlePage);
			await testShell.GoToAsync("ModalTestPage");
			tab.NavigationsFired.Clear();

			await testShell.GoToAsync("../..");
			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//rootpage"));

			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[0]);
			Assert.AreEqual("OnPopModal", tab.NavigationsFired[1]);
			Assert.AreEqual(2, tab.NavigationsFired.Count);
		}



		[Test]
		public async Task MultiplePopsRemoveMiddlePagesBeforeFinalPopWhenUsingModal()
		{
			Routing.RegisterRoute("ModalTestPage", typeof(ShellModalTests.ModalTestPage));
			TestShell testShell = new TestShell(
				CreateShellSection<NavigationMonitoringTab>(shellContentRoute: "rootpage")
			);

			var pageLeftOnStack = new ContentPage();
			var middlePage = new ContentPage();
			var tab = (NavigationMonitoringTab)testShell.CurrentItem.CurrentItem;
			await testShell.Navigation.PushAsync(pageLeftOnStack);
			await testShell.Navigation.PushAsync(middlePage);
			await testShell.GoToAsync("ModalTestPage");
			tab.NavigationsFired.Clear();

			await testShell.GoToAsync("../..");

			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//rootpage/{Routing.GetRoute(pageLeftOnStack)}"));

			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[0]);
			Assert.AreEqual("OnPopModal", tab.NavigationsFired[1]);
			Assert.AreEqual(2, tab.NavigationsFired.Count);
		}


		[Test]
		public async Task SwappingOutVisiblePageDoesntRevealPreviousPage()
		{
			TestShell testShell = new TestShell(
				CreateShellSection<NavigationMonitoringTab>(shellContentRoute: "rootpage")
			);


			testShell.RegisterPage("firstPage");
			testShell.RegisterPage("pageToSwapIn");

			var tab = (NavigationMonitoringTab)testShell.CurrentItem.CurrentItem;
			await testShell.GoToAsync("firstPage");
			tab.NavigationsFired.Clear();

			await testShell.GoToAsync($"../pageToSwapIn");
			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//rootpage/pageToSwapIn"));

			Assert.AreEqual("OnPushAsync", tab.NavigationsFired[0]);
			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[1]);
			Assert.AreEqual(2, tab.NavigationsFired.Count);
		}


		[Test]
		public async Task MiddleRoutesAreRemovedWithoutPoppingStack()
		{
			TestShell testShell = new TestShell(
				CreateShellSection<NavigationMonitoringTab>(shellContentRoute: "rootpage")
			);

			testShell.RegisterPage("firstPage");
			testShell.RegisterPage("secondPage");
			testShell.RegisterPage("thirdPage");
			testShell.RegisterPage("fourthPage");
			testShell.RegisterPage("fifthPage");

			var tab = (NavigationMonitoringTab)testShell.CurrentItem.CurrentItem;
			await testShell.GoToAsync("firstPage/secondPage/thirdPage/fourthPage/fifthPage");
			tab.NavigationsFired.Clear();

			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//rootpage/firstPage/secondPage/thirdPage/fourthPage/fifthPage"));

			await testShell.GoToAsync($"//rootpage/thirdPage/fifthPage");
			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//rootpage/thirdPage/fifthPage"));

			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[0]);
			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[1]);
			Assert.AreEqual("OnRemovePage", tab.NavigationsFired[2]);
			Assert.AreEqual(3, tab.NavigationsFired.Count);
		}

		public class NavigationMonitoringTab : Tab
		{
			public List<string> NavigationsFired = new List<string>();

			public NavigationMonitoringTab()
			{
				Navigation = new NavigationImpl(this, Navigation);
			}

			protected override Task OnPushAsync(Page page, bool animated)
			{
				NavigationsFired.Add(nameof(OnPushAsync));
				return base.OnPushAsync(page, animated);
			}

			protected override void OnRemovePage(Page page)
			{
				base.OnRemovePage(page);
				NavigationsFired.Add(nameof(OnRemovePage));
			}

			protected override Task<Page> OnPopAsync(bool animated)
			{
				NavigationsFired.Add(nameof(OnPopAsync));
				return base.OnPopAsync(animated);
			}

			public class NavigationImpl : NavigationProxy
			{
				readonly NavigationMonitoringTab _navigationMonitoringTab;
				readonly INavigation _navigation;

				public NavigationImpl(
					NavigationMonitoringTab navigationMonitoringTab,
					INavigation navigation)
				{
					_navigationMonitoringTab = navigationMonitoringTab;
					_navigation = navigation;
				}

				protected override IReadOnlyList<Page> GetModalStack() => _navigation.ModalStack;

				protected override IReadOnlyList<Page> GetNavigationStack() => _navigation.NavigationStack;

				protected override void OnInsertPageBefore(Page page, Page before) => _navigation.InsertPageBefore(page, before);

				protected override Task<Page> OnPopAsync(bool animated) => _navigation.PopAsync(animated);

				protected override Task<Page> OnPopModal(bool animated)
				{
					_navigationMonitoringTab.NavigationsFired.Add(nameof(OnPopModal));
					return _navigation.PopModalAsync(animated);
				}

				protected override Task OnPopToRootAsync(bool animated) => _navigation.PopToRootAsync(animated);

				protected override Task OnPushAsync(Page page, bool animated) => _navigation.PushAsync(page, animated);

				protected override Task OnPushModal(Page modal, bool animated)
				{
					_navigationMonitoringTab.NavigationsFired.Add(nameof(OnPushModal));
					return _navigation.PushModalAsync(modal, animated);
				}

				protected override void OnRemovePage(Page page) => _navigation.RemovePage(page);
			}
		}

		ShellNavigatingEventArgs CreateShellNavigatedEventArgs() =>
			new ShellNavigatingEventArgs("..", "../newstate", ShellNavigationSource.Push, true);
	}
}
