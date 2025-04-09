using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.Controls.Platform.Compatibility.ShellFlyoutTemplatedContentRenderer;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using AView = Android.Views.View;
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact(DisplayName = "No crash going back using 'Shell.Current.GoToAsync(\"..\")'")]
		public async Task GoingBackUsingGoToAsyncMethod()
		{
			SetupBuilder();

			var page1 = new ContentPage();

			var page2Content = new Label { Text = "Test" };
			var page2 = new ContentPage { Content = page2Content };

			var pointerGestureRecognizer = new PointerGestureRecognizer();
			pointerGestureRecognizer.PointerPressed += (sender, args) =>
			{
				Console.WriteLine("Page Content pressed");
			};

			page2Content.GestureRecognizers.Add(pointerGestureRecognizer);

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							Route = "Item1",
							Content = page1
						},
						new ShellContent()
						{
							Route = "Item2",
							Content = page2
						},
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(page1);
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("..");

				await shell.GoToAsync("//Item1");
				await shell.GoToAsync("//Item2");
				await shell.Navigation.PopAsync();

				await shell.GoToAsync("//Item1");
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("..");
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task CanHideNavBarShadow(bool navBarHasShadow)
		{
			SetupBuilder();

			var contentPage = new ContentPage() { Title = "Flyout Item" };
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { contentPage } };

				shell.FlyoutContent = new VerticalStackLayout()
				{
					new Label(){ Text = "Flyout Content"}
				};

				Shell.SetNavBarHasShadow(contentPage, navBarHasShadow);
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await Task.Delay(100);

				var platformToolbar = GetPlatformToolbar(handler);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();

				if (navBarHasShadow)
					Assert.True(appBar.Elevation > 0);
				else
					Assert.True(appBar.Elevation == 0);
			});
		}

		protected async Task CheckFlyoutState(ShellRenderer handler, bool desiredState)
		{
			var drawerLayout = GetDrawerLayout(handler);
			var flyout = drawerLayout.GetChildAt(1);

			if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
			{
				Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));
				return;
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();
			flyout.LayoutChange += OnLayoutChanged;

			try
			{
				await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
			}
			catch (TimeoutException)
			{

			}

			flyout.LayoutChange -= OnLayoutChanged;
			Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));

			return;

			void OnLayoutChanged(object sender, Android.Views.View.LayoutChangeEventArgs e)
			{
				if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
				{
					taskCompletionSource.SetResult(true);
					flyout.LayoutChange -= OnLayoutChanged;
				}
			}
		}

		[Fact(DisplayName = "FlyoutItems Render When FlyoutBehavior Starts As Locked")]
		public async Task FlyoutItemsRendererWhenFlyoutBehaviorStartsAsLocked()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var dl = GetDrawerLayout(handler);
				var flyoutContainer = GetFlyoutMenuReyclerView(handler);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}


		[Fact(DisplayName = "Shell with Flyout Disabled Doesn't Render Flyout")]
		public async Task ShellWithFlyoutDisabledDoesntRenderFlyout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, (handler) =>
			{
				var dl = GetDrawerLayout(handler);
				Assert.Equal(1, dl.ChildCount);
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				Assert.Equal(2, dl.ChildCount);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "FooterTemplate Measures to Set Flyout Width When Flyout Locked")]
		public async Task FooterTemplateMeasuresToSetFlyoutWidth()
		{
			SetupBuilder();
			VerticalStackLayout footer = new VerticalStackLayout()
			{
				new Label(){ Text = "Hello there"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.FlyoutWidth = 20;
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(footer);
				Assert.True(Math.Abs(20 - footer.Frame.Width) < 1);
				Assert.True(footer.Frame.Height > 0);
			});
		}

		[Fact(DisplayName = "Flyout Footer and Default Flyout Items Render")]
		public async Task FlyoutFooterRenderersWithDefaultFlyoutItems()
		{
			SetupBuilder();
			VerticalStackLayout footer = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var dl = GetDrawerLayout(handler);
				await OpenFlyout(handler);

				var flyoutContainer = GetFlyoutMenuReyclerView(handler);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact]
		public async Task FlyoutItemsRenderWhenFlyoutHeaderIsSet()
		{
			SetupBuilder();
			VerticalStackLayout header = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = header;
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var dl = GetDrawerLayout(handler);
				await OpenFlyout(handler);

				var flyoutContainer = GetFlyoutMenuReyclerView(handler);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact]
		public async Task FlyoutHeaderRendersCorrectSizeWithFlyoutContentSet()
		{
			SetupBuilder();
			VerticalStackLayout header = new VerticalStackLayout()
			{
				new Label() { Text = "Flyout Header"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = header;

				shell.FlyoutContent = new VerticalStackLayout()
				{
					new Label(){ Text = "Flyout Content"}
				};

				shell.FlyoutFooter = new VerticalStackLayout()
				{
					new Label(){ Text = "Flyout Footer"}
				};
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var headerPlatformView = header.ToPlatform();
				var appBar = headerPlatformView.GetParentOfType<AppBarLayout>();
				Assert.Equal(appBar.MeasuredHeight, headerPlatformView.MeasuredHeight);
			});
		}

		[Fact]
		public async Task SwappingOutAndroidContextDoesntCrash()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });
			});

			var window = new Controls.Window(shell);
			var mauiContextStub1 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub1);

			var mauiContextStub2 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem1");
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub2);
		}

		[Fact]
		public async Task ChangingBottomTabAttributesDoesntRecreateBottomTabs()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var menu = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>().Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var icon1 = menuItem1.Icon;
				var icon2 = menuItem2.Icon;
				var title1 = menuItem1.TitleFormatted;
				var title2 = menuItem2.TitleFormatted;

				shell.CurrentItem.Items[0].Title = "new Title 1";
				shell.CurrentItem.Items[0].Icon = "blue.png";

				shell.CurrentItem.Items[1].Title = "new Title 2";
				shell.CurrentItem.Items[1].Icon = "blue.png";

				// let the icon and title propagate
				await AssertEventually(() => menuItem1.Icon != icon1);

				menu = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>().Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menuItem1.Icon.AssertColorAtCenter(Android.Graphics.Color.Blue);
				menuItem2.Icon.AssertColorAtCenter(Android.Graphics.Color.Blue);

				Assert.NotEqual(icon1, menuItem1.Icon);
				Assert.NotEqual(icon2, menuItem2.Icon);
				Assert.NotEqual(title1, menuItem1.TitleFormatted);
				Assert.NotEqual(title2, menuItem2.TitleFormatted);
			});
		}

		[Fact]
		public async Task RemovingBottomTabDoesntRecreateMenu()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var bottomView = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);

				shell.CurrentItem.Items.RemoveAt(2);

				// let the change propagate
				await AssertEventually(() => bottomView.Menu.Size() == 2);

				menu = bottomView.Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));
			});
		}

		[Fact]
		public async Task AddingBottomTabDoesntRecreateMenu()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var bottomView = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var menuItem2Icon = menuItem2.Icon;

				shell.CurrentItem.Items.Insert(1, new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "green.png" });

				// let the change propagate
				await AssertEventually(() => bottomView.Menu.GetItem(1).Icon != menuItem2Icon);

				menu = bottomView.Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menu.GetItem(1).Icon.AssertColorAtCenter(Android.Graphics.Color.Green);
				menu.GetItem(2).Icon.AssertColorAtCenter(Android.Graphics.Color.Red);
			});
		}

		protected AView GetFlyoutPlatformView(ShellRenderer shellRenderer)
		{
			var drawerLayout = GetDrawerLayout(shellRenderer);
			return drawerLayout.GetChildrenOfType<ShellFlyoutLayout>().First();
		}

		internal Graphics.Rect GetFlyoutFrame(ShellRenderer shellRenderer)
		{
			var platformView = GetFlyoutPlatformView(shellRenderer);
			var context = platformView.Context;

			return new Graphics.Rect(0, 0,
				context.FromPixels(platformView.MeasuredWidth),
				context.FromPixels(platformView.MeasuredHeight));
		}

		internal Graphics.Rect GetFrameRelativeToFlyout(ShellRenderer shellRenderer, IView view)
		{
			var platformView = (view.Handler as IPlatformViewHandler).PlatformView;
			return platformView.GetFrameRelativeTo(GetFlyoutPlatformView(shellRenderer));
		}

		protected async Task OpenFlyout(ShellRenderer shellRenderer, TimeSpan? timeOut = null)
		{
			var flyoutView = GetFlyoutPlatformView(shellRenderer);
			var drawerLayout = GetDrawerLayout(shellRenderer);

			if (!drawerLayout.FlyoutFirstDrawPassFinished)
				await Task.Delay(10);

			var hamburger =
				GetPlatformToolbar((IPlatformViewHandler)shellRenderer).GetChildrenOfType<AppCompatImageButton>().FirstOrDefault() ??
				throw new InvalidOperationException("Unable to find Drawer Button");

			timeOut = timeOut ?? TimeSpan.FromSeconds(2);

			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			drawerLayout.DrawerOpened += OnDrawerOpened;
			hamburger.PerformClick();

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);

			void OnDrawerOpened(object sender, DrawerLayout.DrawerOpenedEventArgs e)
			{
				drawerLayout.DrawerOpened -= OnDrawerOpened;
				taskCompletionSource.SetResult(true);
			}
		}

		protected async Task<double> ScrollFlyoutToBottom(ShellRenderer shellRenderer)
		{
			IShellContext shellContext = shellRenderer;
			DrawerLayout dl = shellContext.CurrentDrawerLayout;
			var viewGroup = dl.GetChildAt(1) as ViewGroup;
			var scrollView = viewGroup?.GetChildAt(0);

			if (scrollView is RecyclerView rv)
			{
				return await ScrollRecyclerViewToBottom(rv);
			}
			else if (scrollView is MauiScrollView mauisv)
			{
				return await ScrollMauiScrollViewToBottom(mauisv);
			}
			else
			{
				throw new Exception("RecyclerView or MauiScrollView not found");
			}
		}

		async Task<double> ScrollRecyclerViewToBottom(RecyclerView flyoutItems)
		{
			TaskCompletionSource<object> result = new TaskCompletionSource<object>();
			flyoutItems.ScrollChange += OnFlyoutItemsScrollChange;
			flyoutItems.ScrollToPosition(flyoutItems.GetAdapter().ItemCount - 1);
			await result.Task.WaitAsync(TimeSpan.FromSeconds(2));
			await Task.Delay(10);

			void OnFlyoutItemsScrollChange(object sender, AView.ScrollChangeEventArgs e)
			{
				flyoutItems.ScrollChange -= OnFlyoutItemsScrollChange;
				result.TrySetResult(true);
			}

			// The appbar layout won't offset if you programmatically scroll the RecyclerView
			// I haven't found a way to match the exact behavior when you touch and scroll
			// I think we'd have to actually send touch events through adb

			var coordinatorLayout = flyoutItems.Parent.GetParentOfType<CoordinatorLayout>();
			var appbarLayout = coordinatorLayout.GetFirstChildOfType<AppBarLayout>();
			var clLayoutParams = appbarLayout.LayoutParameters as CoordinatorLayout.LayoutParams;
			var behavior = clLayoutParams.Behavior as AppBarLayout.Behavior;
			var headerContainer = appbarLayout.GetFirstChildOfType<HeaderContainer>();

			var verticalOffset = flyoutItems.ComputeVerticalScrollOffset();
			behavior.OnNestedPreScroll(coordinatorLayout, appbarLayout, flyoutItems, 0, verticalOffset, new int[2], ViewCompat.TypeTouch);
			await Task.Delay(10);

			return verticalOffset;
		}

		async Task<double> ScrollMauiScrollViewToBottom(MauiScrollView flyoutItems)
		{
			TaskCompletionSource<object> result = new TaskCompletionSource<object>();
			flyoutItems.ScrollChange += OnFlyoutItemsScrollChange;
			flyoutItems.ScrollTo(0, flyoutItems.Height);
			await result.Task.WaitAsync(TimeSpan.FromSeconds(2));
			await Task.Delay(10);

			void OnFlyoutItemsScrollChange(object sender, NestedScrollView.ScrollChangeEventArgs e)
			{
				flyoutItems.ScrollChange -= OnFlyoutItemsScrollChange;
				result.TrySetResult(true);
			}

			// The appbar layout won't offset if you programmatically scroll the RecyclerView
			// I haven't found a way to match the exact behavior when you touch and scroll
			// I think we'd have to actually send touch events through adb

			var coordinatorLayout = flyoutItems.Parent.GetParentOfType<CoordinatorLayout>();
			var appbarLayout = coordinatorLayout.GetFirstChildOfType<AppBarLayout>();
			var clLayoutParams = appbarLayout.LayoutParameters as CoordinatorLayout.LayoutParams;
			var behavior = clLayoutParams.Behavior as AppBarLayout.Behavior;
			var headerContainer = appbarLayout.GetFirstChildOfType<HeaderContainer>();

#pragma warning disable XAOBS001 // Obsolete
			var verticalOffset = flyoutItems.ComputeVerticalScrollOffset();
#pragma warning restore XAOBS001 // Obsolete
			behavior.OnNestedPreScroll(coordinatorLayout, appbarLayout, flyoutItems, 0, verticalOffset, new int[2], ViewCompat.TypeTouch);
			await Task.Delay(10);

			return verticalOffset;
		}

		ShellFlyoutRenderer GetDrawerLayout(ShellRenderer shellRenderer)
		{
			IShellContext shellContext = shellRenderer;
			return (ShellFlyoutRenderer)shellContext.CurrentDrawerLayout;
		}

		RecyclerView GetFlyoutMenuReyclerView(ShellRenderer shellRenderer)
		{
			IShellContext shellContext = shellRenderer;
			DrawerLayout dl = shellContext.CurrentDrawerLayout;

			var flyout = dl.GetChildAt(0);
			RecyclerView flyoutContainer = null;

			var ViewGroup = dl.GetChildAt(1) as ViewGroup;
			var rc = ViewGroup.GetChildAt(0) as RecyclerView;

			if (dl.GetChildAt(1) is ViewGroup vg1 &&
				vg1.GetChildAt(0) is RecyclerView rvc)
			{
				flyoutContainer = rvc;
			}

			return flyoutContainer ?? throw new Exception("RecyclerView not found");
		}

		async Task TapToSelect(ContentPage page)
		{
			var shellContent = page.Parent as ShellContent;
			var shellSection = shellContent.Parent as ShellSection;
			var shellItem = shellSection.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;
			await OnNavigatedToAsync(shell.CurrentPage);

			if (shellItem != shell.CurrentItem)
				throw new NotImplementedException();

			if (shellSection != shell.CurrentItem.CurrentItem)
				throw new NotImplementedException();

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.GetParentOfType<ViewPager2>();

			pagerParent.CurrentItem = shellSection.Items.IndexOf(shellContent);
			await OnNavigatedToAsync(page);
		}
	}
}
