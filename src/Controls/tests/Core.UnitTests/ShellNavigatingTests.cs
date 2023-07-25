using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellNavigatingTests : ShellTestBase
	{

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Routing.Clear();
			}

			base.Dispose(disposing);
		}

		[Fact]
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

			Assert.Equal("//one/tabone/content", shell.CurrentState.Location.ToString());

			shell.Navigating += (s, e) =>
			{
				e.Cancel();
			};

			shell.GoToAsync(new ShellNavigationState("//two/tabfour/"));

			Assert.Equal("//one/tabone/content", shell.CurrentState.Location.ToString());
		}

		[Fact]
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

			Assert.True(executed);
			Assert.False(result);
		}

		[Fact]
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

			Assert.True(executed);
			Assert.NotEqual(contentActiveBeforeCompletingDeferral, navigatingToShellContent);
			Assert.Equal(flyoutItem.Items[0].Items[0], contentActiveBeforeCompletingDeferral);
			Assert.Equal(flyoutItem.Items[0].CurrentItem, navigatingToShellContent);
		}

		[Fact]
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
			Assert.True(executed);
			Assert.Equal(2, shell.Navigation.NavigationStack.Count);
		}

		[Theory]
		[InlineData("PopToRoot")]
		[InlineData("Pop")]
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
				Assert.Equal(3, shell.Navigation.NavigationStack.Count);
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
				Assert.Equal(2, shell.Navigation.NavigationStack.Count);
			}
			else
			{
				await shell.Navigation.PopToRootAsync();
				await source.Task;
				Assert.Single(shell.Navigation.NavigationStack);
			}
		}

		[Theory]
		[InlineData("PopToRoot")]
		[InlineData("Pop")]
		[InlineData("GoToAsync")]
		[InlineData("Push")]
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

			Assert.True(_token.IsCompleted);
		}

		[Fact]
		public void CompletingTheSameDeferralTokenTwiceDoesntDoAnything()
		{
			var args = CreateShellNavigatedEventArgs();
			var token = args.GetDeferral();
			args.GetDeferral();

			Assert.Equal(2, args.DeferralCount);
			token.Complete();
			Assert.Equal(1, args.DeferralCount);
			token.Complete();
			Assert.Equal(1, args.DeferralCount);
		}

		[Fact]
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

			Assert.Equal(shell.CurrentState.Location.ToString(), $"//{itemRoute}");

			await shell.Navigation.PushAsync(page1);

			Assert.Equal(shell.CurrentState.Location.ToString(), $"//{itemRoute}/{Routing.GetRoute(page1)}");

			await shell.Navigation.PushAsync(page2);

			Assert.Equal(shell.CurrentState.Location.ToString(), $"//{itemRoute}/{Routing.GetRoute(page1)}/{Routing.GetRoute(page2)}");

			await shell.GoToAsync("..");

			Assert.Equal(shell.CurrentState.Location.ToString(), $"//{itemRoute}/{Routing.GetRoute(page1)}");
		}

		[Fact]
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

			Assert.Equal("//item/pagefirstmiddle/middle/pagesecondmiddle/last", shell.CurrentState.Location.ToString());
		}

		[Fact]
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

			Assert.Equal(shell.CurrentState.Location.ToString(),
				$"//{itemRoute}");

			await shell.Navigation.PushAsync(page1);

			Assert.Equal(shell.CurrentState.Location.ToString(),
				$"//{itemRoute}/{Routing.GetRoute(page1)}");

			await shell.Navigation.PushAsync(page2);

			Assert.Equal(shell.CurrentState.Location.ToString(),
				$"//{itemRoute}/{Routing.GetRoute(page1)}/{Routing.GetRoute(page2)}");

			await shell.Navigation.PopAsync();

			Assert.Equal(shell.CurrentState.Location.ToString(),
				$"//{itemRoute}/{Routing.GetRoute(page1)}");
		}

		[Fact]
		public async Task NavigateToDefaultShellContent()
		{
			TestShell testShell = new TestShell(CreateShellItem<FlyoutItem>());
			var page = new ContentPage();

			var contentRoute = testShell.CurrentItem.CurrentItem.CurrentItem.Route;
			var pageRoute = Routing.GetRoute(page);

			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PushAsync(page);

			await testShell.GoToAsync($"//{contentRoute}/{pageRoute}");


			Assert.Equal(testShell.CurrentState.Location.ToString(),
				$"//{contentRoute}/{pageRoute}");
		}

		[Fact]
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

		[Fact]
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
			Assert.Equal(testShell.CurrentState.Location.ToString(),
				$"//rootpage/{Routing.GetRoute(pageLeftOnStack)}");

			Assert.Equal("OnRemovePage", tab.NavigationsFired[0]);
			Assert.Equal("OnPopAsync", tab.NavigationsFired[1]);
			Assert.Equal(2, tab.NavigationsFired.Count);
		}

		[Fact]
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
			Assert.Equal($"//rootpage", testShell.CurrentState.Location.ToString());

			Assert.Equal("OnRemovePage", tab.NavigationsFired[0]);
			Assert.Equal("OnPopModal", tab.NavigationsFired[1]);
			Assert.Equal(2, tab.NavigationsFired.Count);
		}



		[Fact]
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

			Assert.Equal(testShell.CurrentState.Location.ToString(),
				$"//rootpage/{Routing.GetRoute(pageLeftOnStack)}");

			Assert.Equal("OnRemovePage", tab.NavigationsFired[0]);
			Assert.Equal("OnPopModal", tab.NavigationsFired[1]);
			Assert.Equal(2, tab.NavigationsFired.Count);
		}


		[Fact]
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
			Assert.Equal($"//rootpage/pageToSwapIn", testShell.CurrentState.Location.ToString());

			Assert.Equal("OnPushAsync", tab.NavigationsFired[0]);
			Assert.Equal("OnRemovePage", tab.NavigationsFired[1]);
			Assert.Equal(2, tab.NavigationsFired.Count);
		}


		[Fact]
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

			Assert.Equal($"//rootpage/firstPage/secondPage/thirdPage/fourthPage/fifthPage", testShell.CurrentState.Location.ToString());

			await testShell.GoToAsync($"//rootpage/thirdPage/fifthPage");
			Assert.Equal($"//rootpage/thirdPage/fifthPage", testShell.CurrentState.Location.ToString());

			Assert.Equal("OnRemovePage", tab.NavigationsFired[0]);
			Assert.Equal("OnRemovePage", tab.NavigationsFired[1]);
			Assert.Equal("OnRemovePage", tab.NavigationsFired[2]);
			Assert.Equal(3, tab.NavigationsFired.Count);
		}

		[Fact]
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

		[Theory]
		[InlineData(true, 2)]
		[InlineData(false, 2)]
		[InlineData(true, 3)]
		[InlineData(false, 3)]
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
			Assert.Equal("//animals/monkeys/details", shell.CurrentState.Location.ToString());
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
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
			Assert.Equal("//animals/monkeys/details/details", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task ShellSectionWithGlobalRouteAbsolute()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("edit", typeof(ContentPage));

			shell.Items.Add(item1);

			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("//rootlevelcontent1/edit"));

			Assert.Single(request.Request.GlobalRoutes);
			Assert.Equal("edit", request.Request.GlobalRoutes.First());
		}

		[Fact]
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

			Assert.Equal(editShellContent, shell.CurrentItem.CurrentItem.CurrentItem);
		}


		[Fact]
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


		[Fact]
		public async Task RouteWithGlobalPageRoute()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellItemRoute: "animals", shellSectionRoute: "domestic", shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellItemRoute: "animals", shellSectionRoute: "domestic", shellContentRoute: "cats");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));
			await shell.GoToAsync("//cats/catdetails?name=3");

			Assert.Equal("//animals/domestic/cats/catdetails", shell.CurrentState.Location.ToString());
		}

		[Theory]
		[InlineData(typeof(PageWithDependency))]
		[InlineData(typeof(PageWithDependencyAndMultipleConstructors))]
		[InlineData(typeof(PageWithUnregisteredDependencyAndParameterlessConstructor))]
		public async Task GlobalRouteWithDependencyResolution(Type pageType)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddTransient<Dependency>();
			serviceCollection.AddTransient<PageWithDependency>();
			serviceCollection.AddTransient<PageWithDependencyAndMultipleConstructors>();
			IServiceProvider services = serviceCollection.BuildServiceProvider();
			var fakeMauiContext = Substitute.For<IMauiContext>();
			var fakeHandler = Substitute.For<IElementHandler>();
			fakeMauiContext.Services.Returns(services);
			fakeHandler.MauiContext.Returns(fakeMauiContext);

			var flyoutItem = CreateShellItem<FlyoutItem>();
			flyoutItem.Items.Add(CreateShellContent(asImplicit: true, shellContentRoute: "cats"));
			var shell = new TestShell
			{
				Items = { flyoutItem }
			};
			shell.Parent.Handler = fakeHandler;
			var routeName = pageType.Name;
			Routing.RegisterRoute(routeName, pageType);
			await shell.GoToAsync(routeName);

			Assert.NotNull(shell.Navigation);
			Assert.NotNull(shell.Navigation.NavigationStack);
			var page = shell.Navigation.NavigationStack[1];
			Assert.NotNull(page);
			if (pageType == typeof(PageWithDependency) || pageType == typeof(Dependency))
			{
				Assert.IsType<PageWithDependency>(page);
				Assert.NotNull((page as PageWithDependency).TestDependency);
			}

			if (pageType == typeof(PageWithDependencyAndMultipleConstructors))
			{
				Assert.IsType<PageWithDependencyAndMultipleConstructors>(page);
				var testPage = page as PageWithDependencyAndMultipleConstructors;
				Assert.NotNull(testPage.TestDependency);
				Assert.Null(testPage.OtherTestDependency);
			}

			if (pageType == typeof(PageWithUnregisteredDependencyAndParameterlessConstructor))
			{
				Assert.IsType<PageWithUnregisteredDependencyAndParameterlessConstructor>(page);
			}
		}

		[Fact]
		public async Task AbsoluteRoutingToPage()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellItemRoute: "animals", shellSectionRoute: "domestic", shellContentRoute: "dogs");
			shell.Items.Add(item1);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));

			Assert.ThrowsAnyAsync<Exception>(async () => await shell.GoToAsync($"//catdetails"));
		}

		[Fact]
		public async Task LocationRemovesImplicit()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);

			Assert.Equal("//rootlevelcontent1", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task GlobalNavigateTwice()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);
			Routing.RegisterRoute("cat", typeof(ContentPage));
			Routing.RegisterRoute("details", typeof(ContentPage));

			await shell.GoToAsync("cat");
			await shell.GoToAsync("details");

			Assert.Equal("//rootlevelcontent1/cat/details", shell.CurrentState.Location.ToString());
			await shell.GoToAsync("//rootlevelcontent1/details");
			Assert.Equal("//rootlevelcontent1/details", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task GlobalRoutesRegisteredHierarchicallyNavigateCorrectly()
		{
			Routing.RegisterRoute("first", typeof(TestPage1));
			Routing.RegisterRoute("first/second", typeof(TestPage2));
			Routing.RegisterRoute("first/second/third", typeof(TestPage3));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "MainPage")
			);

			await shell.GoToAsync("//MainPage/first/second");

			Assert.Equal(typeof(TestPage1), shell.Navigation.NavigationStack[1].GetType());
			Assert.Equal(typeof(TestPage2), shell.Navigation.NavigationStack[2].GetType());

			await shell.GoToAsync("//MainPage/first/second/third");

			Assert.Equal(typeof(TestPage1), shell.Navigation.NavigationStack[1].GetType());
			Assert.Equal(typeof(TestPage2), shell.Navigation.NavigationStack[2].GetType());
			Assert.Equal(typeof(TestPage3), shell.Navigation.NavigationStack[3].GetType());
		}

		[Fact]
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
			Assert.Equal("//animals/monkeys/monkeyDetails/monkeygenome", shell.CurrentState.Location.ToString());
		}

		[Fact]
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
			Assert.Equal("//animals/monkeys", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task GlobalRoutesRegisteredHierarchicallyWithDoubleSplash()
		{
			Routing.RegisterRoute("//animals/monkeys/monkeyDetails", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			await shell.GoToAsync("//animals/monkeys/monkeyDetails?id=123");
			Assert.Equal("//animals/monkeys/monkeyDetails", shell.CurrentState.Location.ToString());
		}


		[Fact]
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

		[Fact]
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
			Assert.Equal("//animals/monkeys/monkeyDetails/monkeygenome", shell.CurrentState.Location.ToString());
		}

		[Fact]
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

		[Fact]
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

		[Fact]
		public async Task HierarchicalNavigation()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2?{nameof(ShellTestPage.SomeQueryParameter)}=1");

			Assert.Equal("1", ((ShellTestPage)shell.CurrentPage).SomeQueryParameter);
		}

		[Fact]
		public async Task HierarchicalNavigationMultipleRoutes()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			Routing.RegisterRoute("page1/page2/page3", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2?{nameof(ShellTestPage.SomeQueryParameter)}=1");

			Assert.Equal("1", ((ShellTestPage)shell.CurrentPage).SomeQueryParameter);
			await shell.GoToAsync($"page1/page2/page3");

			Assert.True(shell.CurrentPage is TestPage1);
			Assert.True(shell.Navigation.NavigationStack[1] is ShellTestPage);
		}

		[Fact]
		public async Task HierarchicalNavigationMultipleRoutesVariation1()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			Routing.RegisterRoute("page1/page2/page3", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2/page3");

			Assert.True(shell.CurrentPage is TestPage1);
			Assert.True(shell.Navigation.NavigationStack[1] is ShellTestPage);
		}

		[Fact]
		public async Task HierarchicalNavigationWithBackNavigation()
		{
			Routing.RegisterRoute("page1/page2", typeof(ShellTestPage));
			Routing.RegisterRoute("page1/page2/page3", typeof(TestPage1));
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "page1")
			);

			await shell.GoToAsync($"page1/page2");
			await shell.GoToAsync($"page1/page2/page3");
			Assert.True(shell.CurrentPage is TestPage1);
			await shell.GoToAsync($"..");
			Assert.True(shell.CurrentPage is ShellTestPage);
			await shell.GoToAsync($"..");
			Assert.True(shell.CurrentPage is ContentPage);
		}

		[Fact]
		public async Task NavigatedFiresAfterSwitchingFlyoutItemsBothWithPushedPages()
		{
			var shellContent1 = new ShellContent() { Content = new ContentPage() };
			var shellContent2 = new ShellContent() { Content = new ContentPage() };

			var shell = new TestShell(shellContent1, shellContent2);
			IShellController shellController = shell;
			await shell.Navigation.PushAsync(new ContentPage());
			await shellController.OnFlyoutItemSelectedAsync(shellContent2);
			await shell.Navigation.PushAsync(new ContentPage());
			await shellController.OnFlyoutItemSelectedAsync(shellContent1);

			Assert.Equal(2, shell.Items[0].Items[0].Navigation.NavigationStack.Count);
			Assert.Equal(2, shell.Items[1].Items[0].Navigation.NavigationStack.Count);
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
				readonly NavigationProxy _navigation;

				public NavigationImpl(
					NavigationMonitoringTab navigationMonitoringTab,
					INavigation navigation)
				{
					_navigationMonitoringTab = navigationMonitoringTab;
					_navigation = (NavigationProxy)navigation;
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
