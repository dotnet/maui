using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellNavigatingTests : ShellTestBase
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Routing.Clear();
		}

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

		[Test]
		public void CancelNavigationOccurringOutsideGotoAsyncWithoutDelay()
		{
			var flyoutItem = CreateShellItem<FlyoutItem>();
			TestShell shell = new TestShell()
			{
				Items = { flyoutItem }
			};

			var navigatingToShellContent = CreateShellContent();
			shell.Items[0].Items[0].Items.Add(navigatingToShellContent);

			bool executed = false;
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

			ShellContent contentActiveBeforeCompletingDeferral = null;
			shell.Navigating += (_, args) =>
			{
				var deferral = args.GetDeferral();
				contentActiveBeforeCompletingDeferral = flyoutItem.Items[0].Items[0];
				args.Cancel();
				deferral.Complete();
				executed = true;
			};

			bool result = shell.Controller.ProposeNavigation(
				ShellNavigationSource.ShellContentChanged, flyoutItem, flyoutItem.Items[0], navigatingToShellContent, flyoutItem.Items[0].Stack, true);

			Assert.IsTrue(executed);
			Assert.IsFalse(result);
		}

		[Test]
		public async Task CancelNavigationOccurringOutsideGotoAsync()
		{
			var flyoutItem = CreateShellItem<FlyoutItem>();
			TestShell shell = new TestShell()
			{
				Items = { flyoutItem }
			};

			var navigatingToShellContent = CreateShellContent();
			shell.Items[0].Items[0].Items.Add(navigatingToShellContent);

			bool executed = false;
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

			ShellContent contentActiveBeforeCompletingDeferral = null;

			shell.Navigating += async (_, args) =>
			{
				var deferral = args.GetDeferral();
				await Task.Delay(100);

				contentActiveBeforeCompletingDeferral = flyoutItem.Items[0].Items[0];
				executed = true;
				deferral.Complete();
			};

			shell.Navigated += (_, args) =>
			{
				taskCompletionSource.SetResult(true);
			};

			shell.Controller.ProposeNavigation(
				ShellNavigationSource.ShellContentChanged, flyoutItem, flyoutItem.Items[0], navigatingToShellContent, flyoutItem.Items[0].Stack, true);

			await taskCompletionSource.Task;

			Assert.IsTrue(executed);
			Assert.AreNotEqual(contentActiveBeforeCompletingDeferral, navigatingToShellContent);
			Assert.AreEqual(flyoutItem.Items[0].Items[0], contentActiveBeforeCompletingDeferral, "Navigation to new Content was not deferred");
			Assert.AreEqual(flyoutItem.Items[0].CurrentItem, navigatingToShellContent, "Navigation after completing the deferral failed");
		}

		[Test]
		public async Task ImmediatelyCompleteDeferral()
		{
			TestShell shell = new TestShell()
			{
				Items = { CreateShellItem<FlyoutItem>() }
			};

			bool executed = false;
			shell.Navigating += (_, args) =>
			{
				var deferral = args.GetDeferral();
				executed = true;
				deferral.Complete();
			};

			await shell.Navigation.PushAsync(new ContentPage());
			Assert.IsTrue(executed);
			Assert.AreEqual(2, shell.Navigation.NavigationStack.Count);
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
		public async Task NavigationTaskCompletesAfterDeferralHasFinished(string testCase)
		{
			Routing.RegisterRoute(nameof(NavigationTaskCompletesAfterDeferralHasFinished), typeof(ContentPage));
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
					await shell.GoToAsync(nameof(NavigationTaskCompletesAfterDeferralHasFinished));
					break;
				case "Push":
					await shell.Navigation.PushAsync(new ContentPage());
					break;
			}

			Assert.IsTrue(_token.IsCompleted);
		}

		[Test]
		public void CompletingTheSameDeferralTokenTwiceDoesntDoAnything()
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
		public async Task InsertTwoPagesAtSeparatePoints()
		{
			Routing.RegisterRoute("pagefirstmiddle", typeof(ContentPage));
			Routing.RegisterRoute("pagesecondmiddle", typeof(ContentPage));
			Routing.RegisterRoute("last", typeof(ContentPage));
			Routing.RegisterRoute("middle", typeof(ContentPage));

			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/middle/last");
			await shell.GoToAsync("//item/pagefirstmiddle/middle/pagesecondmiddle/last");

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo("//item/pagefirstmiddle/middle/pagesecondmiddle/last"));
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

		[Test]
		public async Task PoppingSamePageSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("detailspage", typeof(ContentPage));
			var shell = new TestShell(CreateShellItem(shellItemRoute: "item1"));
			await shell.GoToAsync("detailspage/detailspage");
			await shell.Navigation.PopAsync();


			shell.TestNavigatingArgs(ShellNavigationSource.Pop,
				"//item1/detailspage/detailspage", $"..");

			shell.TestNavigatedArgs(ShellNavigationSource.Pop,
				"//item1/detailspage/detailspage", $"//item1/detailspage");
		}

		[Test]
		public async Task PoppingSetsCorrectNavigationSource()
		{
			var shell = new TestShell(CreateShellItem(shellContentRoute: "item1"));
			shell.RegisterPage("page1");
			shell.RegisterPage("page2");

			await shell.GoToAsync("page1");
			await shell.GoToAsync("page2");
			await shell.Navigation.PopAsync();

			shell.TestNavigatingArgs(ShellNavigationSource.Pop,
				"//item1/page1/page2", $"..");

			shell.TestNavigatedArgs(ShellNavigationSource.Pop,
				"//item1/page1/page2", $"//item1/page1");
		}

		[Test]
		public async Task PopToRootSetsCorrectNavigationSource()
		{
			var shell = new TestShell(CreateShellItem());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PopToRootAsync();
			Assert.AreEqual(ShellNavigationSource.PopToRoot, shell.LastShellNavigatingEventArgs.Source);

			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());

			await shell.Navigation.PopAsync();
			Assert.AreEqual(ShellNavigationSource.Pop, shell.LastShellNavigatingEventArgs.Source);

			await shell.Navigation.PopAsync();
			Assert.AreEqual(ShellNavigationSource.PopToRoot, shell.LastShellNavigatingEventArgs.Source);
		}

		[Test]
		public async Task PushingSetsCorrectNavigationSource()
		{
			var shell = new TestShell(CreateShellItem(shellItemRoute: "item1"));
			shell.RegisterPage(nameof(PushingSetsCorrectNavigationSource));
			await shell.GoToAsync(nameof(PushingSetsCorrectNavigationSource));

			shell.TestNavigatingArgs(ShellNavigationSource.Push,
				"//item1", $"{nameof(PushingSetsCorrectNavigationSource)}");

			shell.TestNavigatedArgs(ShellNavigationSource.Push,
				"//item1", $"//item1/{nameof(PushingSetsCorrectNavigationSource)}");
		}

		[Test]
		public async Task ChangingShellItemSetsCorrectNavigationSource()
		{
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item1"),
				CreateShellItem(shellItemRoute: "item2")
			);

			await shell.GoToAsync("//item2");

			shell.TestNavigationArgs(ShellNavigationSource.ShellItemChanged,
				"//item1", "//item2");
		}

		[Test]
		public async Task ChangingShellSectionSetsCorrectNavigationSource()
		{
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "item1")
			);

			shell.Items[0].Items.Add(CreateShellSection(shellContentRoute: "item2"));

			await shell.GoToAsync("//item2");

			shell.TestNavigationArgs(ShellNavigationSource.ShellSectionChanged,
				"//item1", "//item2");
		}

		[Test]
		public async Task ChangingShellContentSetsCorrectNavigationSource()
		{
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "item1")
			);

			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "item2"));

			await shell.GoToAsync("//item2");

			shell.TestNavigationArgs(ShellNavigationSource.ShellContentChanged,
				"//item1", "//item2");
		}

		[Test]
		public async Task InsertPageSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("pagemiddle", typeof(ContentPage));
			Routing.RegisterRoute("page", typeof(ContentPage));
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/page");
			await shell.GoToAsync("//item/pagemiddle/page");

			shell.TestNavigationArgs(ShellNavigationSource.Insert,
				"//item/page", "//item/pagemiddle/page");
		}

		[Test]
		public async Task RemovePageSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("pagemiddle", typeof(ContentPage));
			Routing.RegisterRoute("page", typeof(ContentPage));
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/pagemiddle/page");
			await shell.GoToAsync("//item/page");


			shell.TestNavigationArgs(ShellNavigationSource.Remove,
				"//item/pagemiddle/page", "//item/page");
		}

		[Test]
		public async Task InitialNavigatingArgs()
		{
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			shell.TestNavigationArgs(ShellNavigationSource.ShellItemChanged,
				null, "//item");
		}


		[TestCase(true, 2)]
		[TestCase(false, 2)]
		[TestCase(true, 3)]
		[TestCase(false, 3)]
		public async Task ShellItemContentRouteWithGlobalRouteRelative(bool modal, int depth)
		{
			var shell = new Shell();
			var item1 = CreateShellItem<FlyoutItem>(asImplicit: true, shellItemRoute: "animals", shellContentRoute: "monkeys");

			string route = "monkeys/details";

			if (depth == 3)
			{
				route = "animals/monkeys/details";
			}

			if (modal)
				Routing.RegisterRoute(route, typeof(ShellModalTests.ModalTestPage));
			else
				Routing.RegisterRoute(route, typeof(ContentPage));

			shell.Items.Add(item1);

			await shell.GoToAsync("details");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//animals/monkeys/details"));
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task GotoSameGlobalRoutesCollapsesUriCorrectly(bool modal)
		{
			var shell = new Shell();
			var item1 = CreateShellItem<FlyoutItem>(asImplicit: true, shellItemRoute: "animals", shellContentRoute: "monkeys");

			if (modal)
				Routing.RegisterRoute("details", typeof(ShellModalTests.ModalTestPage));
			else
				Routing.RegisterRoute("details", typeof(ContentPage));

			shell.Items.Add(item1);

			await shell.GoToAsync("details");
			await shell.GoToAsync("details");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//animals/monkeys/details/details"));
		}

		[Test]
		public async Task ShellSectionWithGlobalRouteAbsolute()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("edit", typeof(ContentPage));

			shell.Items.Add(item1);

			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("//rootlevelcontent1/edit"));

			Assert.AreEqual(1, request.Request.GlobalRoutes.Count);
			Assert.AreEqual("edit", request.Request.GlobalRoutes.First());
		}

		[Test]
		public async Task ShellSectionWithRelativeEdit()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");
			var editShellContent = CreateShellContent(shellContentRoute: "edit");


			item1.Items[0].Items.Add(editShellContent);
			shell.Items.Add(item1);

			await shell.GoToAsync("//rootlevelcontent1");
			var location = shell.CurrentState.FullLocation;
			await shell.NavigationManager.GoToAsync("edit", false, true);

			Assert.AreEqual(editShellContent, shell.CurrentItem.CurrentItem.CurrentItem);
		}


		[Test]
		public async Task ShellContentOnlyWithGlobalEdit()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("//rootlevelcontent1/edit", typeof(ContentPage));
			await shell.GoToAsync("//rootlevelcontent1/edit");
		}


		[Test]
		public async Task RouteWithGlobalPageRoute()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellItemRoute: "animals", shellSectionRoute: "domestic", shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellItemRoute: "animals", shellSectionRoute: "domestic", shellContentRoute: "cats");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));
			await shell.GoToAsync("//cats/catdetails?name=3");

			Assert.AreEqual("//animals/domestic/cats/catdetails", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task AbsoluteRoutingToPage()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellItemRoute: "animals", shellSectionRoute: "domestic", shellContentRoute: "dogs");
			shell.Items.Add(item1);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));

			Assert.That(async () => await shell.GoToAsync($"//catdetails"), Throws.Exception);
		}

		[Test]
		public async Task LocationRemovesImplicit()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);

			Assert.AreEqual("//rootlevelcontent1", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task GlobalNavigateTwice()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);
			Routing.RegisterRoute("cat", typeof(ContentPage));
			Routing.RegisterRoute("details", typeof(ContentPage));

			await shell.GoToAsync("cat");
			await shell.GoToAsync("details");

			Assert.AreEqual("//rootlevelcontent1/cat/details", shell.CurrentState.Location.ToString());
			await shell.GoToAsync("//rootlevelcontent1/details");
			Assert.AreEqual("//rootlevelcontent1/details", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task GlobalRoutesRegisteredHierarchicallyNavigateCorrectly()
		{
			Routing.RegisterRoute("first", typeof(TestPage1));
			Routing.RegisterRoute("first/second", typeof(TestPage2));
			Routing.RegisterRoute("first/second/third", typeof(TestPage3));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "MainPage")
			);

			await shell.GoToAsync("//MainPage/first/second");

			Assert.AreEqual(typeof(TestPage1), shell.Navigation.NavigationStack[1].GetType());
			Assert.AreEqual(typeof(TestPage2), shell.Navigation.NavigationStack[2].GetType());

			await shell.GoToAsync("//MainPage/first/second/third");

			Assert.AreEqual(typeof(TestPage1), shell.Navigation.NavigationStack[1].GetType());
			Assert.AreEqual(typeof(TestPage2), shell.Navigation.NavigationStack[2].GetType());
			Assert.AreEqual(typeof(TestPage3), shell.Navigation.NavigationStack[3].GetType());
		}

		[Test]
		public async Task GlobalRoutesRegisteredHierarchicallyNavigateCorrectlyVariation()
		{
			Routing.RegisterRoute("monkeys/monkeyDetails", typeof(TestPage1));
			Routing.RegisterRoute("monkeyDetails/monkeygenome", typeof(TestPage2));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals2"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			await shell.GoToAsync("//animals/monkeys/monkeyDetails?id=123");
			await shell.GoToAsync("monkeygenome");
			Assert.AreEqual("//animals/monkeys/monkeyDetails/monkeygenome", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task GlobalRoutesRegisteredHierarchicallyWithDoublePop()
		{
			Routing.RegisterRoute("monkeys/monkeyDetails", typeof(TestPage1));
			Routing.RegisterRoute("monkeyDetails/monkeygenome", typeof(TestPage2));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals2"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			await shell.GoToAsync("//animals/monkeys/monkeyDetails?id=123");
			await shell.GoToAsync("monkeygenome");
			await shell.GoToAsync("../..");
			Assert.AreEqual("//animals/monkeys", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task GlobalRoutesRegisteredHierarchicallyWithDoubleSplash()
		{
			Routing.RegisterRoute("//animals/monkeys/monkeyDetails", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			await shell.GoToAsync("//animals/monkeys/monkeyDetails?id=123");
			Assert.AreEqual("//animals/monkeys/monkeyDetails", shell.CurrentState.Location.ToString());
		}


		[Test]
		public async Task RemovePageWithNestedRoutes()
		{
			Routing.RegisterRoute("monkeys/monkeyDetails", typeof(TestPage1));
			Routing.RegisterRoute("monkeyDetails/monkeygenome", typeof(TestPage2));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			await shell.GoToAsync("//animals/monkeys/monkeyDetails");
			await shell.GoToAsync("monkeygenome");
			shell.Navigation.RemovePage(shell.Navigation.NavigationStack[1]);
			await shell.Navigation.PopAsync();
		}

		[Test]
		public async Task GlobalRoutesRegisteredHierarchicallyNavigateCorrectlyWithAdditionalItems()
		{
			Routing.RegisterRoute("monkeys/monkeyDetails", typeof(TestPage1));
			Routing.RegisterRoute("monkeyDetails/monkeygenome", typeof(TestPage2));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "cats", shellSectionRoute: "domestic", shellItemRoute: "animals")
			);

			shell.Items[0].Items.Add(CreateShellContent(shellContentRoute: "monkeys"));
			shell.Items[0].Items.Add(CreateShellContent(shellContentRoute: "elephants"));
			shell.Items[0].Items.Add(CreateShellContent(shellContentRoute: "bears"));
			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "dogs"));
			shell.Items.Add(CreateShellContent(shellContentRoute: "about"));
			await shell.GoToAsync("//animals/monkeys/monkeyDetails?id=123");
			await shell.GoToAsync("monkeygenome");
			Assert.AreEqual("//animals/monkeys/monkeyDetails/monkeygenome", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task GoBackFromRouteWithMultiplePaths()
		{
			Routing.RegisterRoute("monkeys/monkeyDetails", typeof(TestPage1));

			var shell = new TestShell(
				CreateShellItem()
			);

			await shell.GoToAsync("monkeys/monkeyDetails");
			await shell.GoToAsync("monkeys/monkeyDetails");
			await shell.Navigation.PopAsync();
			await shell.Navigation.PopAsync();
		}

		[Test]
		public async Task GoBackFromRouteWithMultiplePathsHierarchical()
		{
			Routing.RegisterRoute("monkeys/monkeyDetails", typeof(TestPage1));
			Routing.RegisterRoute("monkeyDetails/monkeygenome", typeof(TestPage2));

			var shell = new TestShell(
				CreateShellItem()
			);

			await shell.GoToAsync("monkeys/monkeyDetails");
			await shell.GoToAsync("monkeygenome");
			await shell.Navigation.PopAsync();
			await shell.Navigation.PopAsync();
		}

		[Test]
		public async Task HierarchicalNavigation()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2?{nameof(ShellTestPage.SomeQueryParameter)}=1");

			Assert.AreEqual("1", ((ShellTestPage)shell.CurrentPage).SomeQueryParameter);
		}

		[Test]
		public async Task HierarchicalNavigationMultipleRoutes()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			Routing.RegisterRoute("page1/page2/page3", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2?{nameof(ShellTestPage.SomeQueryParameter)}=1");

			Assert.AreEqual("1", ((ShellTestPage)shell.CurrentPage).SomeQueryParameter);
			await shell.GoToAsync($"page1/page2/page3");

			Assert.IsTrue(shell.CurrentPage is TestPage1);
			Assert.IsTrue(shell.Navigation.NavigationStack[1] is ShellTestPage);
		}

		[Test]
		public async Task HierarchicalNavigationMultipleRoutesVariation1()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			Routing.RegisterRoute("page1/page2/page3", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2/page3");

			Assert.IsTrue(shell.CurrentPage is TestPage1);
			Assert.IsTrue(shell.Navigation.NavigationStack[1] is ShellTestPage);
		}

		[Test]
		public async Task HierarchicalNavigationWithBackNavigation()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			Routing.RegisterRoute("page1/page2/page3", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2");
			await shell.GoToAsync($"page1/page2/page3");
			Assert.IsTrue(shell.CurrentPage is TestPage1);
			await shell.GoToAsync($"..");
			Assert.IsTrue(shell.CurrentPage is ShellTestPage);
			await shell.GoToAsync($"..");
			Assert.IsTrue(shell.CurrentPage is ContentPage);
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
