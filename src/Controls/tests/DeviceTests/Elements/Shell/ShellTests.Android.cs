using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Platform;
using Xunit;
using AView = Android.Views.View;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using static Microsoft.Maui.Controls.Platform.Compatibility.ShellFlyoutTemplatedContentRenderer;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
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
			var mauiContextStub1 = new ContextStub(MauiContext.GetApplicationServices());
			var activity = mauiContextStub1.GetActivity();
			mauiContextStub1.Context = new ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub1);

			var mauiContextStub2 = new ContextStub(MauiContext.GetApplicationServices());
			mauiContextStub2.Context = new ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem1");
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub2);
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

		protected async Task ScrollFlyoutToBottom(ShellRenderer shellRenderer)
		{
			var flyoutItems = GetFlyoutMenuReyclerView(shellRenderer);

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
		}

		ShellFlyoutRenderer GetDrawerLayout(ShellRenderer shellRenderer)
		{
			IShellContext shellContext = shellRenderer;
			return (ShellFlyoutRenderer)shellContext.CurrentDrawerLayout;
		}

		RecyclerViewContainer GetFlyoutMenuReyclerView(ShellRenderer shellRenderer)
		{
			IShellContext shellContext = shellRenderer;
			DrawerLayout dl = shellContext.CurrentDrawerLayout;

			var flyout = dl.GetChildAt(0);
			RecyclerViewContainer flyoutContainer = null;

			if (dl.GetChildAt(1) is ViewGroup vg1 &&
				vg1.GetChildAt(0) is RecyclerViewContainer rvc)
			{
				flyoutContainer = rvc;
			}

			return flyoutContainer ?? throw new Exception("RecyclerView not found");
		}
	}
}
