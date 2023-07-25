using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellModalTests : ShellTestBase
	{
		[Fact]
		public async Task AppearingAndDisappearingFireOnMultipleModals()
		{
			var windowPage = new ContentPage();
			var modalPage1 = new ContentPage();
			var modalPage2 = new ContentPage();

			int modal1Appearing = 0;
			int modal1Disappearing = 0;
			int modal2Appearing = 0;
			int modal2Disappearing = 0;
			int windowAppearing = 0;
			int windowDisappearing = 0;

			modalPage1.Appearing += (_, _) => modal1Appearing++;
			modalPage1.Disappearing += (_, _) => modal1Disappearing++;

			modalPage2.Appearing += (_, _) => modal2Appearing++;
			modalPage2.Disappearing += (_, _) => modal2Disappearing++;

			windowPage.Appearing += (_, _) => windowAppearing++;
			windowPage.Disappearing += (_, _) => windowDisappearing++;

			var window = new TestWindow(new TestShell() { CurrentItem = windowPage });
			await windowPage.Navigation.PushModalAsync(modalPage1);
			Assert.Equal(1, modal1Appearing);

			await windowPage.Navigation.PushModalAsync(modalPage2);
			Assert.Equal(1, modal2Appearing);
			Assert.Equal(1, modal1Disappearing);

			await windowPage.Navigation.PopModalAsync();
			await windowPage.Navigation.PopModalAsync();

			Assert.Equal(2, modal1Appearing);
			Assert.Equal(2, modal1Disappearing);

			Assert.Equal(1, modal2Appearing);
			Assert.Equal(1, modal2Disappearing);

			Assert.Equal(2, windowAppearing);
			Assert.Equal(1, windowDisappearing);
		}

		[Fact]
		public async Task BasicModalBehaviorTest()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());

			await shell.GoToAsync("ModalTestPage");

			var navStack = shell.Items[0].Items[0].Navigation;

			Assert.Single(navStack.ModalStack);
			Assert.Equal(typeof(ModalTestPage), navStack.ModalStack[0].GetType());
		}


		[Fact]
		public async Task ModalPopsWhenSwitchingShellItem()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Empty(navStack.ModalStack);
		}

		[Fact]
		public async Task ModalPopsWhenSwitchingShellSection()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());
			shell.Items[0].Items.Add(CreateShellSection(shellSectionRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");
			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Empty(navStack.ModalStack);
		}

		[Fact]
		public async Task AbsoluteRoutingToRootPopsModalPages()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "MainContent"));

			await shell.GoToAsync($"ModalTestPage/ModalTestPage");
			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Equal(2, navStack.ModalStack.Count);

			await shell.GoToAsync($"///MainContent");
			navStack = shell.Items[0].Items[0].Navigation;
			Assert.Empty(navStack.ModalStack);
		}

		[Fact]
		public async Task PoppingEntireModalStackDoesntFireAppearingOnMiddlePages()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "MainContent"));

			await shell.GoToAsync($"ModalTestPage2/ModalTestPage");
			bool appearing = false;
			shell.Items[0].Items[0].Navigation.ModalStack[0].Appearing += (_, __) => appearing = true;
			await shell.GoToAsync($"///MainContent");
			Assert.False(appearing);
		}

		[Fact]
		public async Task PoppingModalStackFiresAppearingOnRevealedModalPage()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "MainContent"));

			await shell.GoToAsync($"ModalTestPage2/ModalTestPage");
			bool appearing = false;
			shell.Items[0].Items[0].Navigation.ModalStack[0].Appearing += (_, __) => appearing = true;

			await shell.Navigation.PopModalAsync();
			Assert.True(true);
		}


		[Fact]
		public async Task ModalPopsWhenSwitchingShellContent()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());
			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Empty(navStack.ModalStack);
		}

		[Fact]
		public async Task ModalPopsWhenNavigatingWithoutModalRoute()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Empty(navStack.ModalStack);
		}


		[Fact]
		public async Task ModalPopsWhenNavigatingToNewModalRoute()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute/ModalTestPage2");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Single(navStack.ModalStack);
			Assert.Equal(typeof(ModalTestPage2), navStack.ModalStack[0].GetType());
		}

		[Fact]
		public async Task PagesPushToModalStack()
		{
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalNavigationTestPage/ContentPage");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Equal(typeof(ModalTestPage), navStack.ModalStack[0].Navigation.NavigationStack[0].GetType());
			Assert.Equal(typeof(ContentPage), navStack.ModalStack[0].Navigation.NavigationStack[1].GetType());

			Assert.Equal("//NewRoute/Section/Content/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task MultipleModalStacks()
		{
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalTestPage/ModalNavigationTestPage/ContentPage");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.Equal(typeof(ModalTestPage), navStack.ModalStack[0].GetType());
			Assert.Equal(typeof(ModalTestPage), navStack.ModalStack[1].Navigation.NavigationStack[0].GetType());
			Assert.Equal(typeof(ContentPage), navStack.ModalStack[1].Navigation.NavigationStack[1].GetType());

			Assert.Equal("//NewRoute/Section/Content/ModalTestPage/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task MultipleModalStacksWithContentPageAlreadyPushed()
		{
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ContentPage/ModalNavigationTestPage/ContentPage/ModalNavigationTestPage/ContentPage");
			Assert.Equal("//NewRoute/Section/Content/ContentPage/ModalNavigationTestPage/ContentPage/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}


		[Fact]
		public async Task SwitchingModalStackAbsoluteNavigation()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalNavigationTestPage/ContentPage/ModalNavigationTestPage/ContentPage");
			await shell.GoToAsync("//NewRoute/ModalNavigationTestPage/ContentPage");

			Assert.Equal("//NewRoute/Section/Content/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task SwitchingShellSectionsAndPushingModal()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content1"));
			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "Content2"));
			await shell.GoToAsync("//Content2/ModalNavigationTestPage");

			Assert.Equal("//NewRoute/Section/Content2/ModalNavigationTestPage", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task PushingNonNavigationPage()
		{
			Shell shell = new TestShell();
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("//NewRoute/SomeCustomPage/ModalNavigationTestPage/ContentPage");

			Assert.Equal("//NewRoute/Section/Content/SomeCustomPage/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}


		[Fact]
		public async Task PushingMultipleVersionsOfTheModalRoute()
		{
			Shell shell = new TestShell();
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalTestPage");
			Assert.Equal("//NewRoute/Section/Content/ModalTestPage", shell.CurrentState.Location.ToString());

			await shell.GoToAsync("ModalTestPage");
			Assert.Equal("//NewRoute/Section/Content/ModalTestPage/ModalTestPage", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public Task PushingContentPageToNonNavigationPageThrowsException() => DispatcherTest.Run(async () =>
		{
			Shell shell = new TestShell();
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			bool invalidOperationThrown = true;
			DispatcherProviderStubOptions.InvokeOnMainThread = (action) =>
			{
				try
				{
					action();
				}
				catch (InvalidOperationException)
				{
					invalidOperationThrown = true;
				}
			};

			Assert.True(invalidOperationThrown);
		});


		[Fact]
		public async Task AppearingAndDisappearingFiresOnShellWithModal()
		{
			Shell shell = new TestShell();
			shell.NavigationProxy.Inner = new NavigationProxy();
			var lifeCyclePage = new ShellLifeCycleTests.LifeCyclePage();
			shell.Items.Add(CreateShellItem(lifeCyclePage, shellItemRoute: "item", shellSectionRoute: "section", shellContentRoute: "content"));

			var shellLifeCycleState = new ShellLifeCycleTests.ShellLifeCycleState(shell);
			await shell.GoToAsync("ModalTestPage");
			await shell.Navigation.ModalStack[0].Navigation.PopModalAsync();
			shellLifeCycleState.AllTrue();
			await shell.GoToAsync("ModalTestPage");
			shellLifeCycleState.AllFalse();
		}


		[Fact]
		public async Task IsAppearingFiredOnLastModalPageOnly()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalTestPage/ModalTestPage2");

			var page1 = (ShellLifeCycleTests.LifeCyclePage)shell.Navigation.ModalStack[0];
			var page2 = (ShellLifeCycleTests.LifeCyclePage)shell.Navigation.ModalStack[1];

			Assert.False(page1.Appearing);
			Assert.True(page2.Appearing);
		}

		[Fact]
		public async Task ParentSetsWhenPushingAndUnsetsWhenPopping()
		{
			var shell = new TestShell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			shell.Items.Add(item);
			await shell.GoToAsync($"ModalTestPage");
			var modal1 = (shell.CurrentItem.CurrentItem as IShellSectionController).PresentedPage as ModalTestPageBase;
			Assert.Equal(shell.Window, modal1.Parent);
			await shell.GoToAsync("..");
			Assert.Null(modal1.Parent);
		}

		[Fact]
		public async Task BasicQueryStringTest()
		{
			var shell = new TestShell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			shell.Items.Add(item);
			await shell.GoToAsync(new ShellNavigationState($"ModalTestPage?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			var testPage = (shell.CurrentItem.CurrentItem as IShellSectionController).PresentedPage as ModalTestPageBase;
			Assert.Equal("1234", testPage.SomeQueryParameter);
		}

		[Theory]
		[InlineData("..")]
		[InlineData("../")]
		public async Task PoppingWithQueryString(string input)
		{
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			var shell = new TestShell(CreateShellItem());

			await shell.GoToAsync("details");
			await shell.GoToAsync("ModalTestPage");

			await shell.GoToAsync(new ShellNavigationState($"{input}?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			shell.AssertCurrentStateEquals($"//{shell.CurrentItem.CurrentItem.CurrentItem.Route}/details");

			var testPage = shell.CurrentPage as ShellTestPage;
			Assert.Equal("1234", testPage.SomeQueryParameter);
		}

		[Fact]
		public async Task NavigatingAndNavigatedFiresForShellModal()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			ShellNavigatingEventArgs shellNavigatingEventArgs = null;
			ShellNavigatedEventArgs shellNavigatedEventArgs = null;

			shell.Navigating += (_, args) =>
			{
				shellNavigatingEventArgs = args;
			};

			shell.Navigated += (_, args) =>
			{
				shellNavigatedEventArgs = args;
			};

			await shell.GoToAsync("ModalTestPage");

			Assert.NotNull(shellNavigatingEventArgs);
			Assert.NotNull(shellNavigatedEventArgs);

			Assert.Equal("//NewRoute/Section/Content", shellNavigatingEventArgs.Current.FullLocation.ToString());
			Assert.Equal("//NewRoute/Section/Content/ModalTestPage", shellNavigatedEventArgs.Current.FullLocation.ToString());

		}

		[Fact]
		public async Task GetCurrentPageInModalNavigation()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			Page page = null;

			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};

			await shell.GoToAsync("ModalTestPage");
			Assert.NotNull(page);
			Assert.IsType<ModalTestPage>(page);
		}

		[Fact]
		public async Task PopModalWithDots()
		{
			Shell shell = new TestShell();
			shell.Items.Add(CreateShellItem());

			await shell.CurrentPage.Navigation.PushModalAsync(new ContentPage());
			await shell.CurrentPage.Navigation.PushModalAsync(new ContentPage());
			await shell.GoToAsync("..");
			Assert.Single(shell.Navigation.ModalStack);
			await shell.GoToAsync("..");
			Assert.Empty(shell.Navigation.ModalStack);
		}

		[Fact]
		public async Task CanCancelGoToModalAsync()
		{
			TestShell shell = new TestShell();
			shell.Items.Add(CreateShellItem());

			shell.Navigating += async (_, args) =>
			{
				var deferral = args.GetDeferral();
				await Task.Delay(10);
				args.Cancel();
				deferral.Complete();
			};

			await shell.GoToAsync("ModalTestPage");
			Assert.Empty(shell.Navigation.ModalStack);
		}

		[Fact]
		public async Task CanCancelPushModalAsync()
		{
			TestShell shell = new TestShell();

			shell.Items.Add(CreateShellItem());
			shell.Navigating += async (_, args) =>
			{
				var deferral = args.GetDeferral();
				await Task.Delay(10);
				args.Cancel();
				deferral.Complete();
			};

			await shell.CurrentPage.Navigation.PushModalAsync(new ContentPage());
			Assert.Empty(shell.Navigation.ModalStack);
		}

		[Fact]
		public async Task PopModalFromShellNavigationProxy()
		{
			Routing.RegisterRoute("ModalTestPage", typeof(ModalTestPage));
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			await shell.GoToAsync("ModalTestPage");
			await shell.Navigation.PopModalAsync();

			Assert.Equal("//NewRoute", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task PushModalFromShellNavigationProxy()
		{
			ModalTestPage modalTestPage = new ModalTestPage();
			Routing.SetRoute(modalTestPage, "ModalTestPage");

			Routing.RegisterRoute("ModalTestPage", typeof(ModalTestPage));
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));
			await shell.Navigation.PushModalAsync(modalTestPage);

			Assert.Equal("//NewRoute/ModalTestPage", shell.CurrentState.Location.ToString());
		}

		[QueryProperty("SomeQueryParameter", "SomeQueryParameter")]
		public class ModalTestPageBase : ShellLifeCycleTests.LifeCyclePage
		{
			public string SomeQueryParameter
			{
				get;
				set;
			}

			public ModalTestPageBase()
			{
				Shell.SetPresentationMode(this, PresentationMode.Modal);
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
			}


			protected override void OnParentSet()
			{
				base.OnParentSet();
			}
		}

		public class ModalTestPage : ModalTestPageBase
		{
		}

		public class ModalTestPage2 : ModalTestPageBase
		{
		}

		public class ModalNavigationTestPage : NavigationPage
		{
			public ModalNavigationTestPage() : base(new ModalTestPage())
			{
				Shell.SetPresentationMode(this, PresentationMode.Modal);
			}
		}

		public class SomeCustomPage : Page
		{
			public SomeCustomPage()
			{
				Shell.SetPresentationMode(this, PresentationMode.Modal);
			}
		}

		public ShellModalTests()
		{

			Routing.RegisterRoute("ModalTestPage", typeof(ModalTestPage));
			Routing.RegisterRoute("ModalTestPage2", typeof(ModalTestPage2));
			Routing.RegisterRoute("SomeCustomPage", typeof(SomeCustomPage));
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Routing.RegisterRoute("LifeCyclePage", typeof(ShellLifeCycleTests.LifeCyclePage));
			Routing.RegisterRoute("ModalNavigationTestPage", typeof(ModalNavigationTestPage));
		}
	}
}
