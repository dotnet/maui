using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellModalTests : ShellTestBase
	{
		[Test]
		public async Task BasicModalBehaviorTest()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());

			await shell.GoToAsync("ModalTestPage");

			var navStack = shell.Items[0].Items[0].Navigation;

			Assert.AreEqual(1, navStack.ModalStack.Count);
			Assert.AreEqual(typeof(ModalTestPage), navStack.ModalStack[0].GetType());
		}


		[Test]
		public async Task ModalPopsWhenSwitchingShellItem()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(0, navStack.ModalStack.Count);
		}

		[Test]
		public async Task ModalPopsWhenSwitchingShellSection()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.Items[0].Items.Add(CreateShellSection(shellSectionRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");
			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(0, navStack.ModalStack.Count);
		}

		[Test]
		public async Task AbsoluteRoutingToRootPopsModalPages()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "MainContent"));

			await shell.GoToAsync($"ModalTestPage/ModalTestPage");
			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(2, navStack.ModalStack.Count);

			await shell.GoToAsync($"///MainContent");
			navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(0, navStack.ModalStack.Count);
		}

		[Test]
		public async Task PoppingEntireModalStackDoesntFireAppearingOnMiddlePages()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "MainContent"));

			await shell.GoToAsync($"ModalTestPage2/ModalTestPage");
			bool appearing = false;
			shell.Items[0].Items[0].Navigation.ModalStack[0].Appearing += (_, __) => appearing = true;
			await shell.GoToAsync($"///MainContent");
			Assert.IsFalse(appearing);
		}

		[Test]
		public async Task PoppingModalStackFiresAppearingOnRevealedModalPage()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "MainContent"));

			await shell.GoToAsync($"ModalTestPage2/ModalTestPage");
			bool appearing = false;
			shell.Items[0].Items[0].Navigation.ModalStack[0].Appearing += (_, __) => appearing = true;

			await shell.Navigation.PopModalAsync();
			Assert.IsTrue(true);
		}


		[Test]
		public async Task ModalPopsWhenSwitchingShellContent()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(0, navStack.ModalStack.Count);
		}

		[Test]
		public async Task ModalPopsWhenNavigatingWithoutModalRoute()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(0, navStack.ModalStack.Count);
		}


		[Test]
		public async Task ModalPopsWhenNavigatingToNewModalRoute()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute"));

			// pushes modal onto visible shell section
			await shell.GoToAsync("ModalTestPage");

			// Navigates to different Shell Item
			await shell.GoToAsync("///NewRoute/ModalTestPage2");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(1, navStack.ModalStack.Count);
			Assert.AreEqual(typeof(ModalTestPage2), navStack.ModalStack[0].GetType());
		}

		[Test]
		public async Task PagesPushToModalStack()
		{
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalNavigationTestPage/ContentPage");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(typeof(ModalTestPage), navStack.ModalStack[0].Navigation.NavigationStack[0].GetType());
			Assert.AreEqual(typeof(ContentPage), navStack.ModalStack[0].Navigation.NavigationStack[1].GetType());

			Assert.AreEqual("//NewRoute/Section/Content/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task MultipleModalStacks()
		{
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalTestPage/ModalNavigationTestPage/ContentPage");

			var navStack = shell.Items[0].Items[0].Navigation;
			Assert.AreEqual(typeof(ModalTestPage), navStack.ModalStack[0].GetType());
			Assert.AreEqual(typeof(ModalTestPage), navStack.ModalStack[1].Navigation.NavigationStack[0].GetType());
			Assert.AreEqual(typeof(ContentPage), navStack.ModalStack[1].Navigation.NavigationStack[1].GetType());

			Assert.AreEqual("//NewRoute/Section/Content/ModalTestPage/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task MultipleModalStacksWithContentPageAlreadyPushed()
		{
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ContentPage/ModalNavigationTestPage/ContentPage/ModalNavigationTestPage/ContentPage");
			Assert.AreEqual("//NewRoute/Section/Content/ContentPage/ModalNavigationTestPage/ContentPage/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}


		[Test]
		public async Task SwitchingModalStackAbsoluteNavigation()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalNavigationTestPage/ContentPage/ModalNavigationTestPage/ContentPage");
			await shell.GoToAsync("//NewRoute/ModalNavigationTestPage/ContentPage");

			Assert.AreEqual("//NewRoute/Section/Content/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task SwitchingShellSectionsAndPushingModal()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content1"));
			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "Content2"));
			await shell.GoToAsync("//Content2/ModalNavigationTestPage");

			Assert.AreEqual("//NewRoute/Section/Content2/ModalNavigationTestPage", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task PushingNonNavigationPage()
		{
			Shell shell = new Shell();
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("//NewRoute/SomeCustomPage/ModalNavigationTestPage/ContentPage");

			Assert.AreEqual("//NewRoute/Section/Content/SomeCustomPage/ModalNavigationTestPage/ContentPage", shell.CurrentState.Location.ToString());
		}


		[Test]
		public async Task PushingMultipleVersionsOfTheModalRoute()
		{
			Shell shell = new Shell();
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalTestPage");
			Assert.AreEqual("//NewRoute/Section/Content/ModalTestPage", shell.CurrentState.Location.ToString());

			await shell.GoToAsync("ModalTestPage");
			Assert.AreEqual("//NewRoute/Section/Content/ModalTestPage/ModalTestPage", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task PushingContentPageToNonNavigationPageThrowsException()
		{
			Shell shell = new Shell();
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			bool invalidOperationThrown = true;
			Device.PlatformServices = new MockPlatformServices(invokeOnMainThread: (action) =>
			{
				try
				{
					action();
				}
				catch (InvalidOperationException)
				{
					invalidOperationThrown = true;
				}
			});

			Assert.IsTrue(invalidOperationThrown);
		}


		[Test]
		public async Task AppearingAndDisappearingFiresOnShellWithModal()
		{
			Shell shell = new Shell();
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


		[Test]
		public async Task IsAppearingFiredOnLastModalPageOnly()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			await shell.GoToAsync("ModalTestPage/ModalTestPage2");

			var page1 = (ShellLifeCycleTests.LifeCyclePage)shell.Navigation.ModalStack[0];
			var page2 = (ShellLifeCycleTests.LifeCyclePage)shell.Navigation.ModalStack[1];

			Assert.IsFalse(page1.Appearing);
			Assert.IsTrue(page2.Appearing);
		}

		[Test]
		public async Task BasicQueryStringTest()
		{
			var shell = new Shell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			shell.Items.Add(item);
			await shell.GoToAsync(new ShellNavigationState($"ModalTestPage?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			var testPage = (shell.CurrentItem.CurrentItem as IShellSectionController).PresentedPage as ModalTestPageBase;
			Assert.AreEqual("1234", testPage.SomeQueryParameter);
		}

		[Test]
		public async Task NavigatingAndNavigatedFiresForShellModal()
		{
			Shell shell = new Shell();
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

			Assert.IsNotNull(shellNavigatingEventArgs, "Shell.Navigating never fired");
			Assert.IsNotNull(shellNavigatedEventArgs, "Shell.Navigated never fired");

			Assert.AreEqual("//NewRoute/Section/Content", shellNavigatingEventArgs.Current.FullLocation.ToString());
			Assert.AreEqual("//NewRoute/Section/Content/ModalTestPage", shellNavigatedEventArgs.Current.FullLocation.ToString());

		}

		[Test]
		public async Task GetCurrentPageInModalNavigation()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem(shellItemRoute: "NewRoute", shellSectionRoute: "Section", shellContentRoute: "Content"));

			Page page = null;

			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};

			await shell.GoToAsync("ModalTestPage");
			Assert.IsNotNull(page);
			Assert.AreEqual(page.GetType(), typeof(ModalTestPage));
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

		public override void Setup()
		{
			base.Setup();
			Routing.RegisterRoute("ModalTestPage", typeof(ModalTestPage));
			Routing.RegisterRoute("ModalTestPage2", typeof(ModalTestPage2));
			Routing.RegisterRoute("SomeCustomPage", typeof(SomeCustomPage));
			Routing.RegisterRoute("ContentPage", typeof(ContentPage));
			Routing.RegisterRoute("LifeCyclePage", typeof(ShellLifeCycleTests.LifeCyclePage));
			Routing.RegisterRoute("ModalNavigationTestPage", typeof(ModalNavigationTestPage));
		}
	}
}