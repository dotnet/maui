using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class NavigationUnitTest : BaseTestFixture
	{
		[TestCase(false)]
		[TestCase(true)]
		public async Task HandlerUpdatesDontFireForLegacy(bool withPage)
		{
			TestNavigationPage nav =
				new TestNavigationPage(false, (withPage) ? new ContentPage() : null);

			var handler = new TestNavigationHandler();
			(nav as IView).Handler = handler;


			Assert.IsNull(nav.CurrentNavigationTask);
			Assert.IsNull(handler.CurrentNavigationRequest);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task HandlerUpdatesFireWithStartingPage(bool withPage)
		{
			TestNavigationPage nav =
				new TestNavigationPage(true, (withPage) ? new ContentPage() : null);

			var handler = new TestNavigationHandler();
			(nav as IView).Handler = handler;

			if (!withPage)
			{
				Assert.IsNull(nav.CurrentNavigationTask);
			}
			else
			{
				Assert.IsNotNull(nav.CurrentNavigationTask);
			}
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestNavigationImplPush(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Assert.IsNull(nav.RootPage);
			Assert.IsNull(nav.CurrentPage);

			Label child = new Label { Text = "Label" };
			Page childRoot = new ContentPage { Content = child };

			await nav.Navigation.PushAsync(childRoot);

			Assert.AreSame(childRoot, nav.RootPage);
			Assert.AreSame(childRoot, nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestNavigationImplPop(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Label child = new Label();
			Page childRoot = new ContentPage { Content = child };

			Label child2 = new Label();
			Page childRoot2 = new ContentPage { Content = child2 };

			await nav.Navigation.PushAsync(childRoot);
			await nav.Navigation.PushAsync(childRoot2);

			bool fired = false;
			nav.Popped += (sender, e) => fired = true;

			Assert.AreSame(childRoot, nav.RootPage);
			Assert.AreNotSame(childRoot2, nav.RootPage);
			Assert.AreNotSame(nav.RootPage, nav.CurrentPage);

			var popped = await nav.Navigation.PopAsync();

			Assert.True(fired);
			Assert.AreSame(childRoot, nav.RootPage);
			Assert.AreSame(childRoot, nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
			Assert.AreEqual(childRoot2, popped);

			await nav.PopAsync();
			var last = await nav.Navigation.PopAsync();

			Assert.IsNull(last);
			Assert.IsNotNull(nav.RootPage);
			Assert.IsNotNull(nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestPushRoot(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Assert.IsNull(nav.RootPage);
			Assert.IsNull(nav.CurrentPage);

			Label child = new Label { Text = "Label" };
			Page childRoot = new ContentPage { Content = child };

			await nav.PushAsync(childRoot);

			Assert.AreSame(childRoot, nav.RootPage);
			Assert.AreSame(childRoot, nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestPushEvent(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Label child = new Label();
			Page childRoot = new ContentPage { Content = child };

			bool fired = false;
			nav.Pushed += (sender, e) => fired = true;

			await nav.PushAsync(childRoot);

			Assert.True(fired);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestDoublePush(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Label child = new Label();
			Page childRoot = new ContentPage { Content = child };

			await nav.PushAsync(childRoot);

			bool fired = false;
			nav.Pushed += (sender, e) => fired = true;

			Assert.AreSame(childRoot, nav.RootPage);
			Assert.AreSame(childRoot, nav.CurrentPage);

			await nav.PushAsync(childRoot);

			Assert.False(fired);
			Assert.AreSame(childRoot, nav.RootPage);
			Assert.AreSame(childRoot, nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestPop(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Label child = new Label();
			Page childRoot = new ContentPage { Content = child };

			Label child2 = new Label();
			Page childRoot2 = new ContentPage { Content = child2 };

			await nav.PushAsync(childRoot);
			await nav.PushAsync(childRoot2);

			bool fired = false;
			nav.Popped += (sender, e) => fired = true;
			var popped = await nav.PopAsync();

			Assert.True(fired);
			Assert.AreSame(childRoot, nav.CurrentPage);
			Assert.AreEqual(childRoot2, popped);

			await nav.PopAsync();
			var last = await nav.PopAsync();

			Assert.IsNull(last);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestPopToRoot(bool useMaui)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage { Content = new View() };
			var child1 = new ContentPage { Content = new View() };
			var child2 = new ContentPage { Content = new View() };

			await nav.PushAsync(root);
			await nav.PushAsync(child1);
			await nav.PushAsync(child2);

			await nav.PopToRootAsync();

			Assert.True(signaled);
			Assert.AreSame(root, nav.RootPage);
			Assert.AreSame(root, nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestPopToRootEventArgs(bool useMaui)
		{
			var nav = new TestNavigationPage(useMaui);

			List<Page> poppedChildren = null;
			nav.PoppedToRoot += (sender, args) => poppedChildren = (args as PoppedToRootEventArgs).PoppedPages.ToList();

			var root = new ContentPage { Content = new View() };
			var child1 = new ContentPage { Content = new View() };
			var child2 = new ContentPage { Content = new View() };

			await nav.PushAsync(root);
			await nav.PushAsync(child1);
			await nav.PushAsync(child2);

			await nav.PopToRootAsync();

			Assert.IsNotNull(poppedChildren);
			Assert.AreEqual(2, poppedChildren.Count);
			Assert.Contains(child1, poppedChildren);
			Assert.Contains(child2, poppedChildren);
			Assert.AreSame(root, nav.RootPage);
			Assert.AreSame(root, nav.CurrentPage);
			Assert.AreSame(nav.RootPage, nav.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task PeekOne(bool useMaui)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage { Content = new View() };
			var child1 = new ContentPage { Content = new View() };
			var child2 = new ContentPage { Content = new View() };

			await nav.PushAsync(root);
			await nav.PushAsync(child1);
			await nav.PushAsync(child2);

			Assert.AreEqual(((INavigationPageController)nav).Peek(1), child1);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task PeekZero(bool useMaui)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage { Content = new View() };
			var child1 = new ContentPage { Content = new View() };
			var child2 = new ContentPage { Content = new View() };

			await nav.PushAsync(root);
			await nav.PushAsync(child1);
			await nav.PushAsync(child2);

			Assert.AreEqual(((INavigationPageController)nav).Peek(0), child2);
			Assert.AreEqual(((INavigationPageController)nav).Peek(), child2);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task PeekPastStackDepth(bool useMaui)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage { Content = new View() };
			var child1 = new ContentPage { Content = new View() };
			var child2 = new ContentPage { Content = new View() };

			await nav.PushAsync(root);
			await nav.PushAsync(child1);
			await nav.PushAsync(child2);

			Assert.AreEqual(((INavigationPageController)nav).Peek(3), null);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task PeekShallow(bool useMaui)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage { Content = new View() };
			var child1 = new ContentPage { Content = new View() };
			var child2 = new ContentPage { Content = new View() };

			await nav.PushAsync(root);
			await nav.PushAsync(child1);
			await nav.PushAsync(child2);

			Assert.AreEqual(((INavigationPageController)nav).Peek(-1), null);
		}

		[Test]
		public async Task PeekEmpty([Values(true, false)] bool useMaui, [Range(0, 3)] int depth)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			Assert.AreEqual(((INavigationPageController)nav).Peek(depth), null);
		}


		[TestCase(false)]
		[TestCase(true)]
		public void ConstructWithRoot(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			Assert.AreEqual(1, ((INavigationPageController)nav).StackDepth);
			Assert.AreEqual(root, ((IElementController)nav).LogicalChildren[0]);

		}

		[TestCase(false)]
		[TestCase(true)]
		public void TitleViewSetProperty(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			View target = new View();

			NavigationPage.SetTitleView(root, target);

			var result = NavigationPage.GetTitleView(root);

			Assert.AreSame(result, target);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void TitleViewSetsParentWhenAdded(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			View target = new View();

			NavigationPage.SetTitleView(root, target);

			Assert.AreSame(root, target.Parent);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void TitleViewClearsParentWhenRemoved(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			View target = new View();

			NavigationPage.SetTitleView(root, target);

			NavigationPage.SetTitleView(root, null);

			Assert.IsNull(target.Parent);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task NavigationChangedEventArgs(bool useMaui)
		{
			var rootPage = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, rootPage);

			var rootArg = new Page();

			navPage.Pushed += (s, e) =>
			{
				rootArg = e.Page;
			};

			var pushPage = new ContentPage
			{
				Title = "Page 2"
			};

			await navPage.PushAsync(pushPage);

			Assert.AreEqual(rootArg, pushPage);

			var secondPushPage = new ContentPage
			{
				Title = "Page 3"
			};

			await navPage.PushAsync(secondPushPage);

			Assert.AreEqual(rootArg, secondPushPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task CurrentPageChanged(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			bool changing = false;
			navPage.PropertyChanging += (object sender, PropertyChangingEventArgs e) =>
			{
				if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				{
					Assert.That(navPage.CurrentPage, Is.SameAs(root));
					changing = true;
				}
			};

			var next = new ContentPage { Title = "Next" };

			bool changed = false;
			navPage.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				{
					Assert.That(navPage.CurrentPage, Is.SameAs(next));
					changed = true;
				}
			};

			await navPage.PushAsync(next);

			Assert.That(changing, Is.True, "PropertyChanging was not raised for 'CurrentPage'");
			Assert.That(changed, Is.True, "PropertyChanged was not raised for 'CurrentPage'");
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task HandlesPopToRoot(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			await navPage.PushAsync(new ContentPage());
			await navPage.PushAsync(new ContentPage());

			bool popped = false;
			navPage.PoppedToRoot += (sender, args) =>
			{
				popped = true;
			};

			await navPage.Navigation.PopToRootAsync();

			Assert.True(popped);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task SendsBackButtonEventToCurrentPage(bool useMaui)
		{
			var current = new BackButtonPage();
			var navPage = new TestNavigationPage(useMaui, current);

			var emitted = false;
			current.BackPressed += (sender, args) => emitted = true;

			await navPage.SendBackButtonPressedAsync();

			Assert.True(emitted);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task DoesNotSendBackEventToNonCurrentPage(bool useMaui)
		{
			var current = new BackButtonPage();
			var navPage = new TestNavigationPage(useMaui, current);
			navPage.PushAsync(new ContentPage());

			var emitted = false;
			current.BackPressed += (sender, args) => emitted = true;

			await navPage.SendBackButtonPressedAsync();

			Assert.False(emitted);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task NavigatesBackWhenBackButtonPressed(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			await navPage.PushAsync(new ContentPage());

			var result = await navPage.SendBackButtonPressedAsync();

			Assert.AreEqual(root, navPage.CurrentPage);
			Assert.True(result);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task DoesNotNavigatesBackWhenBackButtonPressedIfHandled(bool useMaui)
		{
			var root = new BackButtonPage { Title = "Root" };
			var second = new BackButtonPage() { Handle = true };
			var navPage = new TestNavigationPage(useMaui, root);

			await navPage.PushAsync(second);

			await navPage.SendBackButtonPressedAsync();

			Assert.AreEqual(second, navPage.CurrentPage);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task DoesNotHandleBackButtonWhenNoNavStack(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			var result = await navPage.SendBackButtonPressedAsync();
			Assert.False(result);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void TestInsertPage(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var newPage = new ContentPage();
			var navPage = new TestNavigationPage(useMaui, root);

			navPage.Navigation.InsertPageBefore(newPage, navPage.RootPage);

			Assert.AreSame(newPage, navPage.RootPage);
			Assert.AreNotSame(newPage, navPage.CurrentPage);
			Assert.AreNotSame(navPage.RootPage, navPage.CurrentPage);
			Assert.AreSame(root, navPage.CurrentPage);

			Assert.Throws<ArgumentException>(() =>
			{
				navPage.Navigation.InsertPageBefore(new ContentPage(), new ContentPage());
			});

			Assert.Throws<ArgumentException>(() =>
			{
				navPage.Navigation.InsertPageBefore(navPage.RootPage, navPage.CurrentPage);
			});

			Assert.Throws<ArgumentNullException>(() =>
			{
				navPage.Navigation.InsertPageBefore(null, navPage.CurrentPage);
			});

			Assert.Throws<ArgumentNullException>(() =>
			{
				navPage.Navigation.InsertPageBefore(new ContentPage(), null);
			});
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task TestRemovePage(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var newPage = new ContentPage();
			var navPage = new TestNavigationPage(useMaui, root);
			await navPage.PushAsync(newPage);

			navPage.Navigation.RemovePage(root);

			Assert.AreSame(newPage, navPage.RootPage);
			Assert.AreSame(newPage, navPage.CurrentPage);
			Assert.AreSame(navPage.RootPage, navPage.CurrentPage);
			Assert.AreNotSame(root, navPage.CurrentPage);

			Assert.Throws<ArgumentException>(() =>
			{
				navPage.Navigation.RemovePage(root);
			});

			Assert.Throws<InvalidOperationException>(() =>
			{
				navPage.Navigation.RemovePage(newPage);
			});

			Assert.Throws<ArgumentNullException>(() =>
			{
				navPage.Navigation.RemovePage(null);
			});
		}

		[Test]
		public async Task CurrentPageUpdatesOnPopBeforeAsyncCompletes()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(true, root);
			await navPage.PushAsync(new ContentPage());
			var popped = navPage.PopAsync();
			Assert.AreEqual(navPage.CurrentPage, root);
			await popped;
			Assert.AreEqual(navPage.CurrentPage, root);
		}

		[TestCase(false, Description = "CurrentPage should not be set to null when you attempt to pop the last page")]
		[TestCase(true, Description = "CurrentPage should not be set to null when you attempt to pop the last page")]
		[Property("Bugzilla", 28335)]
		public async Task CurrentPageNotNullPoppingRoot(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);
			var popped = await navPage.PopAsync();
			Assert.That(popped, Is.Null);
			Assert.That(navPage.CurrentPage, Is.SameAs(root));
		}

		[TestCase(false)]
		[TestCase(true)]
		[Property("Bugzilla", 31171)]
		public async Task ReleasesPoppedPage(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			var isFinalized = false;

			await navPage.PushAsync(new PageWithFinalizer(() => isFinalized = true));
			await navPage.PopAsync();

			await Task.Delay(100);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsTrue(isFinalized);
		}

		[Test]
		public async Task PushingPagesWhileNavigating()
		{
			ContentPage contentPage1 = new ContentPage();
			ContentPage contentPage2 = new ContentPage();
			ContentPage contentPage3 = new ContentPage();

			var navigationPage = new TestNavigationPage(true, contentPage1);
			await navigationPage.PushAsync(contentPage2);
			await navigationPage.PushAsync(contentPage3);

			Assert.AreEqual(3, navigationPage.Navigation.NavigationStack.Count);
			Assert.AreEqual(contentPage1, navigationPage.Navigation.NavigationStack[0]);
			Assert.AreEqual(contentPage2, navigationPage.Navigation.NavigationStack[1]);
			Assert.AreEqual(contentPage3, navigationPage.Navigation.NavigationStack[2]);
			navigationPage.ValidateNavigationCompleted();
		}

		[Test]
		public void TabBarSetsOnWindowForSingleNavigationPage()
		{
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var window = new Window(navigationPage);

			Assert.IsNotNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNull(navigationPage.Toolbar);

		}

		[Test]
		public void TabBarSetsOnFlyoutPage()
		{
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var flyoutPage = new FlyoutPage()
			{
				Detail = navigationPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			var window = new Window(flyoutPage);

			Assert.IsNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNull(navigationPage.Toolbar);
			Assert.IsNotNull(flyoutPage.Toolbar);
		}

		[Test]
		public void TabBarSetsOnWindowWithFlyoutPageNestedInTabbedPage()
		{
			// TabbedPage => FlyoutPage => NavigationPage
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var flyoutPage = new FlyoutPage()
			{
				Detail = navigationPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};
			var tabbedPage = new TabbedPage()
			{
				Children =
				{
					flyoutPage
				}
			};

			var window = new Window(tabbedPage);

			Assert.IsNotNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNull(navigationPage.Toolbar);
			Assert.IsNull(flyoutPage.Toolbar);
			Assert.IsNull(tabbedPage.Toolbar);
		}

		[Test]
		public async Task TabBarSetsOnModalPageWhenWindowAlsoHasNavigationPage()
		{
			var window = new Window(new TestNavigationPage(true, new ContentPage()));
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			await window.Navigation.PushModalAsync(navigationPage);

			Assert.IsNotNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNotNull(navigationPage.Toolbar);
		}

		[Test]
		public async Task TabBarSetsOnModalPageForSingleNavigationPage()
		{
			var window = new Window(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			await window.Navigation.PushModalAsync(navigationPage);

			Assert.IsNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNotNull(navigationPage.Toolbar);
		}

		[Test]
		public async Task TabBarSetsOnFlyoutPageInsideModalPage()
		{
			var window = new Window(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var flyoutPage = new FlyoutPage()
			{
				Detail = navigationPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			await window.Navigation.PushModalAsync(flyoutPage);

			Assert.IsNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNull(navigationPage.Toolbar);
			Assert.IsNotNull(flyoutPage.Toolbar);
		}

		[Test]
		public async Task TabBarSetsOnModalPageWithFlyoutPageNestedInTabbedPage()
		{
			// ModalPage => TabbedPage => FlyoutPage => NavigationPage
			var window = new Window(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var flyoutPage = new FlyoutPage()
			{
				Detail = navigationPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};
			var tabbedPage = new TabbedPage()
			{
				Children =
				{
					flyoutPage
				}
			};

			await window.Navigation.PushModalAsync(tabbedPage);

			Assert.IsNull(window.Toolbar);
			Assert.IsNull(contentPage1.Toolbar);
			Assert.IsNull(navigationPage.Toolbar);
			Assert.IsNull(flyoutPage.Toolbar);
			Assert.IsNotNull(tabbedPage.Toolbar);
		}

		[Test]
		public async Task PushingPageBeforeSettingHandlerPropagatesAfterSettingHandler()
		{
			ContentPage contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, setHandler: false);

			await navigationPage.PushAsync(contentPage1);
			(navigationPage as IView).Handler = new TestNavigationHandler();

			var navTask = navigationPage.CurrentNavigationTask;
			Assert.IsNotNull(navTask);
			await navTask;
			navigationPage.ValidateNavigationCompleted();
		}
	}

	internal class BackButtonPage : ContentPage
	{
		public event EventHandler BackPressed;

		public bool Handle = false;

		protected override bool OnBackButtonPressed()
		{
			if (BackPressed != null)
				BackPressed(this, EventArgs.Empty);
			return Handle;
		}
	}

	internal class PageWithFinalizer : Page
	{
		Action OnFinalize;
		public PageWithFinalizer(Action onFinalize)
		{
			OnFinalize = onFinalize;
		}

		~PageWithFinalizer()
		{
			OnFinalize();
		}
	}
}
