using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class NavigationUnitTest : BaseTestFixture
	{
		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task HandlerUpdatesDontFireForLegacy(bool withPage)
		{
			TestNavigationPage nav =
				new TestNavigationPage(false, (withPage) ? new ContentPage() : null);

			var handler = new TestNavigationHandler();
			(nav as IView).Handler = handler;

			Assert.Null(nav.CurrentNavigationTask);
			Assert.Null(handler.CurrentNavigationRequest);
		}

		[Fact]
		public async Task NavigationInLimboCompletesWhenHandlerIsRemoved()
		{
			TestNavigationPage nav =
				new TestNavigationPage(true);

			var task = nav.PushAsync(new ContentPage());
			(nav as IView).Handler = null;
			await task.WaitAsync(TimeSpan.FromMilliseconds(100));

			var handler = new TestNavigationHandler();
			(nav as IView).Handler = handler;

			await nav.PushAsync(new ContentPage());

			Assert.Null(nav.CurrentNavigationTask);
			Assert.Null(handler.CurrentNavigationRequest);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task HandlerUpdatesFireWithStartingPage(bool withPage)
		{
			TestNavigationPage nav =
				new TestNavigationPage(true, (withPage) ? new ContentPage() : null);

			var handler = new TestNavigationHandler();
			(nav as IView).Handler = handler;

			if (!withPage)
			{
				Assert.Null(nav.CurrentNavigationTask);
			}
			else
			{
				Assert.NotNull(nav.CurrentNavigationTask);
			}
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task TestNavigationImplPush(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Assert.Null(nav.RootPage);
			Assert.Null(nav.CurrentPage);

			Label child = new Label { Text = "Label" };
			Page childRoot = new ContentPage { Content = child };

			await nav.Navigation.PushAsync(childRoot);

			Assert.Same(childRoot, nav.RootPage);
			Assert.Same(childRoot, nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.Same(childRoot, nav.RootPage);
			Assert.NotSame(childRoot2, nav.RootPage);
			Assert.NotSame(nav.RootPage, nav.CurrentPage);

			var popped = await nav.Navigation.PopAsync();

			Assert.True(fired);
			Assert.Same(childRoot, nav.RootPage);
			Assert.Same(childRoot, nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
			Assert.Equal(childRoot2, popped);

			await nav.PopAsync();
			var last = await nav.Navigation.PopAsync();

			Assert.Null(last);
			Assert.NotNull(nav.RootPage);
			Assert.NotNull(nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task TestPushRoot(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Assert.Null(nav.RootPage);
			Assert.Null(nav.CurrentPage);

			Label child = new Label { Text = "Label" };
			Page childRoot = new ContentPage { Content = child };

			await nav.PushAsync(childRoot);

			Assert.Same(childRoot, nav.RootPage);
			Assert.Same(childRoot, nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task TestDoublePush(bool useMaui)
		{
			NavigationPage nav = new TestNavigationPage(useMaui);

			Label child = new Label();
			Page childRoot = new ContentPage { Content = child };

			await nav.PushAsync(childRoot);

			bool fired = false;
			nav.Pushed += (sender, e) => fired = true;

			Assert.Same(childRoot, nav.RootPage);
			Assert.Same(childRoot, nav.CurrentPage);

			await nav.PushAsync(childRoot);

			Assert.False(fired);
			Assert.Same(childRoot, nav.RootPage);
			Assert.Same(childRoot, nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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
			Assert.Same(childRoot, nav.CurrentPage);
			Assert.Equal(childRoot2, popped);

			await nav.PopAsync();
			var last = await nav.PopAsync();

			Assert.Null(last);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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
			Assert.Same(root, nav.RootPage);
			Assert.Same(root, nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.NotNull(poppedChildren);
			Assert.Equal(2, poppedChildren.Count);
			Assert.Contains(child1, poppedChildren);
			Assert.Contains(child2, poppedChildren);
			Assert.Same(root, nav.RootPage);
			Assert.Same(root, nav.CurrentPage);
			Assert.Same(nav.RootPage, nav.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.Equal(((INavigationPageController)nav).Peek(1), child1);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.Equal(((INavigationPageController)nav).Peek(0), child2);
			Assert.Equal(((INavigationPageController)nav).Peek(), child2);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.Null(((INavigationPageController)nav).Peek(3));
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.Null(((INavigationPageController)nav).Peek(-1));
		}

		[Theory]
		[InlineData(true, 0)]
		[InlineData(true, 1)]
		[InlineData(true, 2)]
		[InlineData(true, 3)]
		[InlineData(false, 0)]
		[InlineData(false, 1)]
		[InlineData(false, 2)]
		[InlineData(false, 3)]
		public async Task PeekEmpty(bool useMaui, int depth)
		{
			var nav = new TestNavigationPage(useMaui);

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			Assert.Null(((INavigationPageController)nav).Peek(depth));
		}


		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void ConstructWithRoot(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			Assert.Equal(1, ((INavigationPageController)nav).StackDepth);
			Assert.Equal(root, ((IElementController)nav).LogicalChildren[0]);

		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void TitleViewSetProperty(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			View target = new View();

			NavigationPage.SetTitleView(root, target);

			var result = NavigationPage.GetTitleView(root);

			Assert.Same(result, target);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void TitleViewSetsParentWhenAdded(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			View target = new View();

			NavigationPage.SetTitleView(root, target);

			Assert.Same(root, target.Parent);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void TitleViewClearsParentWhenRemoved(bool useMaui)
		{
			var root = new ContentPage();
			var nav = new TestNavigationPage(useMaui, root);

			View target = new View();

			NavigationPage.SetTitleView(root, target);

			NavigationPage.SetTitleView(root, null);

			Assert.Null(target.Parent);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

			Assert.Equal(rootArg, pushPage);

			var secondPushPage = new ContentPage
			{
				Title = "Page 3"
			};

			await navPage.PushAsync(secondPushPage);

			Assert.Equal(rootArg, secondPushPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task CurrentPageChanged(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			bool changing = false;
			navPage.PropertyChanging += (object sender, PropertyChangingEventArgs e) =>
			{
				if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				{
					Assert.Same(navPage.CurrentPage, root);
					changing = true;
				}
			};

			var next = new ContentPage { Title = "Next" };

			bool changed = false;
			navPage.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				{
					Assert.Same(navPage.CurrentPage, next);
					changed = true;
				}
			};

			await navPage.PushAsync(next);

			Assert.True(changing, "PropertyChanging was not raised for 'CurrentPage'");
			Assert.True(changed, "PropertyChanged was not raised for 'CurrentPage'");
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
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

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task SendsBackButtonEventToCurrentPage(bool useMaui)
		{
			var current = new BackButtonPage();
			var navPage = new TestNavigationPage(useMaui, current);

			var emitted = false;
			current.BackPressed += (sender, args) => emitted = true;

			await navPage.SendBackButtonPressedAsync();

			Assert.True(emitted);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task DoesNotSendBackEventToNonCurrentPage(bool useMaui)
		{
			var current = new BackButtonPage();
			var navPage = new TestNavigationPage(useMaui, current);
			await navPage.PushAsync(new ContentPage());

			var emitted = false;
			current.BackPressed += (sender, args) => emitted = true;

			await navPage.SendBackButtonPressedAsync();

			Assert.False(emitted);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task NavigatesBackWhenBackButtonPressed(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			await navPage.PushAsync(new ContentPage());

			var result = await navPage.SendBackButtonPressedAsync();

			Assert.Equal(root, navPage.CurrentPage);
			Assert.True(result);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task DoesNotNavigatesBackWhenBackButtonPressedIfHandled(bool useMaui)
		{
			var root = new BackButtonPage { Title = "Root" };
			var second = new BackButtonPage() { Handle = true };
			var navPage = new TestNavigationPage(useMaui, root);

			await navPage.PushAsync(second);

			await navPage.SendBackButtonPressedAsync();

			Assert.Equal(second, navPage.CurrentPage);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task DoesNotHandleBackButtonWhenNoNavStack(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);

			var result = await navPage.SendBackButtonPressedAsync();
			Assert.False(result);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void TestInsertPage(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var newPage = new ContentPage();
			var navPage = new TestNavigationPage(useMaui, root);

			navPage.Navigation.InsertPageBefore(newPage, navPage.RootPage);

			Assert.Same(newPage, navPage.RootPage);
			Assert.NotSame(newPage, navPage.CurrentPage);
			Assert.NotSame(navPage.RootPage, navPage.CurrentPage);
			Assert.Same(root, navPage.CurrentPage);

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

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public async Task TestRemovePage(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var newPage = new ContentPage();
			var navPage = new TestNavigationPage(useMaui, root);
			await navPage.PushAsync(newPage);

			navPage.Navigation.RemovePage(root);

			Assert.Same(newPage, navPage.RootPage);
			Assert.Same(newPage, navPage.CurrentPage);
			Assert.Same(navPage.RootPage, navPage.CurrentPage);
			Assert.NotSame(root, navPage.CurrentPage);

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

		[Fact]
		public async Task CurrentPageUpdatesOnPopBeforeAsyncCompletes()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(true, root);
			await navPage.PushAsync(new ContentPage());
			var popped = navPage.PopAsync();
			Assert.Equal(navPage.CurrentPage, root);
			await popped;
			Assert.Equal(navPage.CurrentPage, root);
		}

		[InlineData(false)]
		[InlineData(true)]
		[Theory(DisplayName = "CurrentPageNotNullPoppingRoot (Bugzilla 28335)")]
		public async Task CurrentPageNotNullPoppingRoot(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);
			var popped = await navPage.PopAsync();
			Assert.Null(popped);
			Assert.Same(navPage.CurrentPage, root);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory(DisplayName = "CurrentPageNotNullPoppingRoot (Bugzilla 31171)")]
		public async Task ReleasesPoppedPage(bool useMaui)
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new TestNavigationPage(useMaui, root);
			_ = new TestWindow(navPage);

			var isFinalized = false;

			await navPage.PushAsync(new PageWithFinalizer(() => isFinalized = true));
			await navPage.PopAsync();

			await Task.Delay(100);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.True(isFinalized);
		}

		[Fact]
		public async Task PushingPagesWhileNavigating()
		{
			ContentPage contentPage1 = new ContentPage();
			ContentPage contentPage2 = new ContentPage();
			ContentPage contentPage3 = new ContentPage();

			var navigationPage = new TestNavigationPage(true, contentPage1);
			await navigationPage.PushAsync(contentPage2);
			await navigationPage.PushAsync(contentPage3);

			Assert.Equal(3, navigationPage.Navigation.NavigationStack.Count);
			Assert.Equal(contentPage1, navigationPage.Navigation.NavigationStack[0]);
			Assert.Equal(contentPage2, navigationPage.Navigation.NavigationStack[1]);
			Assert.Equal(contentPage3, navigationPage.Navigation.NavigationStack[2]);
			navigationPage.ValidateNavigationCompleted();
		}

		[Fact]
		public void TabBarSetsOnWindowForSingleNavigationPage()
		{
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var window = new TestWindow(navigationPage);

			Assert.NotNull(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.Null(navigationPage.Toolbar);

		}

		[Fact]
		public void TabBarSetsOnFlyoutPage()
		{
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var flyoutPage = new FlyoutPage()
			{
				Detail = navigationPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			var window = new TestWindow(flyoutPage);

			Assert.Null(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.Null(navigationPage.Toolbar);
			Assert.NotNull(flyoutPage.Toolbar);
		}

		[Fact]
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

			var window = new TestWindow(tabbedPage);

			Assert.NotNull(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.Null(navigationPage.Toolbar);
			Assert.Null(flyoutPage.Toolbar);
			Assert.Null(tabbedPage.Toolbar);
		}

		[Fact]
		public async Task TabBarSetsOnModalPageWhenWindowAlsoHasNavigationPage()
		{
			var window = new TestWindow(new TestNavigationPage(true, new ContentPage()));
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			await window.Navigation.PushModalAsync(navigationPage);

			Assert.NotNull(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.NotNull(navigationPage.Toolbar);
		}

		[Fact]
		public async Task TabBarSetsOnModalPageForSingleNavigationPage()
		{
			var window = new TestWindow(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			await window.Navigation.PushModalAsync(navigationPage);

			Assert.Null(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.NotNull(navigationPage.Toolbar);
		}

		[Fact]
		public async Task PopModalWithEmptyStackThrows()
		{
			var window = new TestWindow(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PopModalAsync());
		}

		[Fact]
		public async Task InvalidOperationExceptionIsThrownWhenNavigatingOutsideNavigationPage()
		{
			var window = new TestWindow(new ContentPage());
			var contentPage1 = new ContentPage();
			var contentPage2 = new ContentPage();

			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PushAsync(contentPage1));
			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PopAsync());
			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PopToRootAsync());
			Assert.Throws<InvalidOperationException>(() => window.Navigation.InsertPageBefore(contentPage1, contentPage2));
			Assert.Throws<InvalidOperationException>(() => window.Navigation.RemovePage(contentPage1));
		}

		[Fact]
		public async Task RemoveWrappingIntoNavigationPage()
		{
			var window = new TestWindow(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			await Assert.ThrowsAsync<InvalidOperationException>(() => window.Navigation.PushAsync(contentPage1));
		}

		[Fact]
		public async Task TabBarSetsOnFlyoutPageInsideModalPage()
		{
			var window = new TestWindow(new ContentPage());
			var contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, contentPage1);
			var flyoutPage = new FlyoutPage()
			{
				Detail = navigationPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			await window.Navigation.PushModalAsync(flyoutPage);

			Assert.Null(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.Null(navigationPage.Toolbar);
			Assert.NotNull(flyoutPage.Toolbar);
		}

		[Fact]
		public async Task TabBarSetsOnModalPageWithFlyoutPageNestedInTabbedPage()
		{
			// ModalPage => TabbedPage => FlyoutPage => NavigationPage
			var window = new TestWindow(new ContentPage());
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

			Assert.Null(window.Toolbar);
			Assert.Null(contentPage1.Toolbar);
			Assert.Null(navigationPage.Toolbar);
			Assert.Null(flyoutPage.Toolbar);
			Assert.NotNull(tabbedPage.Toolbar);
		}

		[Fact]
		public async Task PushingPageBeforeSettingHandlerPropagatesAfterSettingHandler()
		{
			ContentPage contentPage1 = new ContentPage();
			var navigationPage = new TestNavigationPage(true, setHandler: false);

			await navigationPage.PushAsync(contentPage1);
			(navigationPage as IView).Handler = new TestNavigationHandler();

			var navTask = navigationPage.CurrentNavigationTask;
			Assert.NotNull(navTask);
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
			if (BackPressed is not null)
			{
				BackPressed(this, EventArgs.Empty);
			}

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
