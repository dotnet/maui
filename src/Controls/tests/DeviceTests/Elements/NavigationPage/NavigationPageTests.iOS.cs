using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData("Push", false)]
		[InlineData("Push", true)]
		[InlineData("Pop", false)]
		[InlineData("Pop", true)]
		[InlineData("PopToRoot", false)]
		[InlineData("PopToRoot", true)]
		public async Task PageLoadsBeforeNavigatedTo(string operation, bool animated)
		{
			SetupBuilder();
			var page = new ContentPage();
			var navPage = new NavigationPage(operation == "PopToRoot" ? page : new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async _ =>
			{
				await OnNavigatedToAsync(navPage.CurrentPage);

				if (operation == "Pop")
					await navPage.PushAsync(page, false);

				if (operation != "Push")
				{
					var unloaded = new TaskCompletionSource();
					page.Unloaded += (_, _) => unloaded.TrySetResult();
					await navPage.PushAsync(new ContentPage(), false);
					await unloaded.Task.WaitAsync(TimeSpan.FromSeconds(5));

					if (operation == "PopToRoot")
						await navPage.PushAsync(new ContentPage(), false);
				}

				var events = new List<string>();
				var loaded = new TaskCompletionSource();
				var navigatedTo = new TaskCompletionSource();
				page.Loaded += (_, _) =>
				{
					events.Add("Loaded");
					loaded.TrySetResult();
				};
				page.NavigatedTo += (_, _) =>
				{
					events.Add("NavigatedTo");
					navigatedTo.TrySetResult();
				};

				switch (operation)
				{
					case "Push":
						await navPage.PushAsync(page, animated);
						break;
					case "Pop":
						await navPage.PopAsync(animated);
						break;
					case "PopToRoot":
						await navPage.PopToRootAsync(animated);
						break;
				}

				await Task.WhenAll(loaded.Task, navigatedTo.Task).WaitAsync(TimeSpan.FromSeconds(5));
				// Allow the queued platform loaded check to run as well.
				await Task.Delay(100);
				Assert.Equal(new[] { "Loaded", "NavigatedTo" }, events);
			});
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(24)]
		[InlineData(56)]
		public async Task TitleViewStaysWithinClippedNavigationBar(double heightRequest)
		{
			SetupBuilder();
			var titleView = new Grid { HeightRequest = heightRequest };
			titleView.Add(new Label { Text = "Title", FontSize = 18 });
			var page = new ContentPage();
			NavigationPage.SetTitleView(page, titleView);
			var navigationPage = new NavigationPage(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navigationPage), async _ =>
			{
				await OnNavigatedToAsync(page);
				var controller = Assert.IsAssignableFrom<UINavigationController>(
					(navigationPage.Handler as IPlatformViewHandler)?.ViewController);
				var bar = controller.NavigationBar;
				bar.ClipsToBounds = true;

				await AssertEventually(() =>
				{
					bar.LayoutIfNeeded();
					var container = controller.TopViewController.NavigationItem.TitleView;
					if (container?.Superview is null || container.Bounds.Height <= 0)
						return false;

					var titleBounds = container.ConvertRectToView(container.Bounds, bar);
					return titleBounds.Top >= -0.5 && titleBounds.Bottom <= bar.Bounds.Height + 0.5;
				}, message: "TitleView must have a positive height and stay within the clipped navigation bar.");
			});
		}

		// Runs against both variants: SetupBuilder() dispatches to whichever
		// RegisterNavigationPageHandlers override is active (NavigationRenderer on the base
		// class, NavigationViewHandler on NavigationPageNavigationHandlerTests). ViewController
		// resolves to the UINavigationController for both: NavigationRenderer.ViewController
		// returns itself, while NavigationViewHandler.ViewController wraps its own
		// UINavigationController.
		[Fact]
		public async Task NavigatingBackViaBackButtonFiresNavigatedEvent()
		{
			SetupBuilder();
			var page = new ContentPage() { Title = "Root Page" };

			var navPage = new NavigationPage(page) { Title = "App Page" };

			await navPage.PushAsync(new ContentPage() { Title = "Second Page" });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(navPage.CurrentPage);

				var navController = (navPage.Handler as IPlatformViewHandler)?.ViewController as UINavigationController;
				Assert.NotNull(navController);

				Assert.False(page.HasNavigatedTo);

				navController.NavigationBar.TapBackButton();
				await OnNavigatedToAsync(page, TimeSpan.FromSeconds(5));

				Assert.True(page.HasNavigatedTo);
			});
		}

		// Handler-only: exercises NavigationViewHandler's programmatic pop path via
		// PopViewController(animated:false). NavigationRenderer doesn't reliably fire
		// NavigatedTo through this path (verified: times out), so this test is
		// Handler-only, bypassing SetupBuilder to hardcode NavigationViewHandler.
		[Fact]
		public async Task Handler_NavigatingBackViaProgrammaticPopFiresNavigatedEvent()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					RegisterCommonHandlers(handlers);
				});
			});

			var page = new ContentPage() { Title = "Root Page" };

			var navPage = new NavigationPage(page) { Title = "App Page" };

			await navPage.PushAsync(new ContentPage() { Title = "Second Page" });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(navPage.CurrentPage);

				var navController = (navPage.Handler as IPlatformViewHandler)?.ViewController as UINavigationController;
				Assert.NotNull(navController);

				Assert.False(page.HasNavigatedTo);

				// Pop via UIKit - this triggers the handler's OnNavigationComplete which calls OnNativePopCompleted
				navController.PopViewController(animated: false);
				await OnNavigatedToAsync(page, TimeSpan.FromSeconds(5));

				Assert.True(page.HasNavigatedTo);
			});
		}

#if IOS
		// https://github.com/dotnet/maui/issues/39210
		// iOS 26 lays out the opaque bar at y=24 in iPhone landscape but starts the page at y=54.
		[Fact]
		public async Task Handler_PageStartsBelowOpaqueNavigationBarInLandscape()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					RegisterCommonHandlers(handlers);
				});
			});

			var page = new ContentPage { Title = "Second Page", SafeAreaEdges = SafeAreaEdges.None, Content = new Label { Text = "Header" } };
			var navPage = new NavigationPage(new ContentPage { Title = "Root Page" }) { BarBackgroundColor = Colors.Blue };
			await navPage.PushAsync(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(page);

				var navController = (UINavigationController)((IPlatformViewHandler)navPage.Handler).ViewController;
				var pageView = ((IPlatformViewHandler)page.Handler).PlatformView;
				var window = pageView.Window;
				Assert.False(navController.NavigationBar.Translucent);

				bool IsLandscape() => window.Bounds.Width > window.Bounds.Height && pageView.Bounds.Width == window.Bounds.Width;

				try
				{
					RequestOrientation(window, UIInterfaceOrientationMask.Landscape);
					await AssertEventually(IsLandscape, timeout: 5000, message: "The window did not rotate to landscape");

					var bar = navController.NavigationBar;
					var barFrame = bar.ConvertRectToView(bar.Bounds, window);
					var pageFrame = pageView.ConvertRectToView(pageView.Bounds, window);
					Assert.True(pageFrame.Top >= barFrame.Bottom - 0.5,
						$"The page starts at y={pageFrame.Top}, under the opaque navigation bar that ends at y={barFrame.Bottom}");
				}
				finally
				{
					RequestOrientation(window, UIInterfaceOrientationMask.Portrait);
					await AssertEventually(() => window.Bounds.Height > window.Bounds.Width, timeout: 5000, message: "The window did not rotate back to portrait");
				}
			});

			static void RequestOrientation(UIWindow window, UIInterfaceOrientationMask orientation)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(16))
				{
					window.WindowScene.RequestGeometryUpdate(new UIWindowSceneGeometryPreferencesIOS(orientation), _ => { });
				}
			}
		}
#endif

		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task InteractivePopGestureRespectsBackButtonPressedWhenNavigationBarHidden(bool backHandled, bool expectedShouldBegin)
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var backHandlingPage = new BackHandlingPage(backHandled);
			NavigationPage.SetHasNavigationBar(backHandlingPage, false);

			var navPage = new NavigationPage(rootPage);
			await navPage.PushAsync(backHandlingPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnNavigatedToAsync(backHandlingPage);

				var navController = Assert.IsAssignableFrom<UINavigationController>(
					(navPage.Handler as IPlatformViewHandler)?.ViewController);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.Equal(expectedShouldBegin, recognizer.Delegate.ShouldBegin(recognizer));
				if (expectedShouldBegin)
				{
					Assert.True(NativeShouldPopItem(navController));
					Assert.Equal(1, backHandlingPage.BackButtonPressedCount);

					// A cancelled gesture finishes by showing the original top controller.
					navController.Delegate.DidShowViewController(navController, navController.TopViewController, false);
					backHandlingPage.BackHandled = true;

					Assert.False(NativeShouldPopItem(navController));
					Assert.Equal(2, backHandlingPage.BackButtonPressedCount);
					Assert.Same(backHandlingPage, navPage.CurrentPage);
				}
				else
				{
					Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				}
			});
		}

		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task InteractivePopGestureRespectsBackButtonPressedWhenNavigationBarVisible(bool backHandled, bool expectedShouldBegin)
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var backHandlingPage = new BackHandlingPage(backHandled);
			var navPage = new NavigationPage(rootPage);
			await navPage.PushAsync(backHandlingPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnNavigatedToAsync(backHandlingPage);

				var navController = Assert.IsAssignableFrom<UINavigationController>(
					(navPage.Handler as IPlatformViewHandler)?.ViewController);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.Equal(expectedShouldBegin, recognizer.Delegate.ShouldBegin(recognizer));
				if (expectedShouldBegin)
				{
					Assert.True(NativeShouldPopItem(navController));
					Assert.Equal(1, backHandlingPage.BackButtonPressedCount);

					// A cancelled gesture finishes by showing the original top controller.
					navController.Delegate.DidShowViewController(navController, navController.TopViewController, false);
					backHandlingPage.BackHandled = true;

					Assert.False(NativeShouldPopItem(navController));
					Assert.Equal(2, backHandlingPage.BackButtonPressedCount);
					Assert.Same(backHandlingPage, navPage.CurrentPage);
				}
				else
				{
					Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				}
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task PrefersLargeTitles(bool enabled)
		{
			SetupBuilder();
			var page = new ContentPage();
			var navPage = new NavigationPage(page) { Title = "App Page" };
			Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(navPage, enabled);

			var largeTitles = await GetValueAsync(navPage, (handler) => (handler.ViewController as UINavigationController).NavigationBar.PrefersLargeTitles);
			Assert.Equal(enabled, largeTitles);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task TranslucentNavigationBar(bool enabled)
		{
			SetupBuilder();
			var page = new ContentPage();
			var navPage = new NavigationPage(page) { Title = "App Page" };
#pragma warning disable CS0618 // Type or member is obsolete
			Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetIsNavigationBarTranslucent(navPage, enabled);
#pragma warning restore CS0618 // Type or member is obsolete

			var translucent = await GetValueAsync(navPage, (handler) => (handler.ViewController as UINavigationController).NavigationBar.Translucent);
			Assert.Equal(enabled, translucent);
		}

		//src/Compatibility/Core/tests/iOS/NavigationTests.cs
		[Fact]
		[Description("Multiple calls to NavigationRenderer.Dispose shouldn't crash")]
		public async Task NavigationRendererDoubleDisposal()
		{
			SetupNavigationRenderer();

			var root = new ContentPage()
			{
				Title = "root",
				Content = new Label { Text = "Hello" }
			};

			await root.Dispatcher.DispatchAsync(() =>
			{
				var navPage = new NavigationPage(false, root);
				var handler = CreateHandler(navPage);

				// Calling Dispose more than once should be fine
				(handler as NavigationRenderer).Dispose();
				(handler as NavigationRenderer).Dispose();
			});
		}

		[Fact]
		[Description("Multiple calls to NavigationViewHandler.DisconnectHandler shouldn't crash")]
		public async Task Handler_NavigationViewHandlerDoubleDisposal()
		{
			// This test targets NavigationViewHandler.DisconnectHandler() specifically, so it
			// always registers NavigationViewHandler directly rather than deferring to
			// SetupBuilder() (which, on the base Renderer class, would register
			// NavigationRenderer instead).
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					RegisterCommonHandlers(handlers);
				});
			});

			var root = new ContentPage()
			{
				Title = "root",
				Content = new Label { Text = "Hello" }
			};

			await root.Dispatcher.DispatchAsync(() =>
			{
				var navPage = new NavigationPage(root);
				var handler = CreateHandler(navPage);

				// Calling DisconnectHandler more than once should be fine
				// NavigationViewHandler uses OnDisconnectHandler lifecycle, not IDisposable
				handler.DisconnectHandler();
				handler.DisconnectHandler();
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NativePopContinuationAfterRendererDisposalDoesNotThrow(bool popToRoot)
		{
			SetupNavigationRenderer();
			var root = new ContentPage();
			var navigationPage = new NavigationPage(root);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navigationPage), async _ =>
			{
				var (renderer, context, poppedPage, poppedController) = await PopAndDismissModalAsync(root, popToRoot);
				try
				{
					// ModalWrapper retains the native renderer until it is collected.
					// Yield so UIKit can release it on the main thread.
					for (var pass = 0; pass < 40 && renderer.Handle != IntPtr.Zero; pass++)
					{
						GC.Collect();
						GC.WaitForPendingFinalizers();
						GC.Collect();
						await Task.Delay(100);
					}
					Assert.Equal(IntPtr.Zero, (IntPtr)renderer.Handle);
					Assert.NotEmpty(context.Tasks);
				}
				finally
				{
					context.Release();
				}

				// Observe the actual continuation tasks instead of relying on the
				// process-wide UnobservedTaskException event and finalizer timing.
				await Task.WhenAll(context.Tasks).WaitAsync(TimeSpan.FromSeconds(5));

				// A cached page must not keep its old controller's subscriptions alive.
				NavigationPage.SetIconColor(poppedPage, Colors.Red);
				poppedPage.Title = "Changed after dismissal";
				poppedPage.ToolbarItems[0].Text = "Changed toolbar item";
				poppedPage.ToolbarItems.Add(new ToolbarItem { Text = "Added after dismissal" });
				await AssertEventually(() => poppedController.Handle == IntPtr.Zero,
					message: "The removed controller should be disposed even when its page is retained.");
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task StatusBarQueriesDuringInitialPushDoNotThrow(bool inFlyoutPage)
		{
			// iOS 27 asks for the status bar child controller while the first page is still being
			// created (#39895, #39669). The root page's HandlerChanged fires in that window.
			SetupNavigationRenderer();
			var root = new ContentPage();
			var navPage = new NavigationPage(root);
			Page container = inFlyoutPage
				? new FlyoutPage { Flyout = new ContentPage { Title = "Flyout" }, Detail = navPage }
				: navPage;
			var reached = new List<UIViewController>();
			Exception error = null;

			root.HandlerChanged += OnRootHandlerChanged;
			void OnRootHandlerChanged(object sender, EventArgs e)
			{
				root.HandlerChanged -= OnRootHandlerChanged;
				try
				{
					var start = ((IPlatformViewHandler)container.Handler).ViewController;
					reached.Add(Walk(start, vc => vc.ChildViewControllerForStatusBarHidden()));
#if !MACCATALYST
					reached.Add(Walk(start, vc => vc.ChildViewControllerForStatusBarStyle()));
#endif
					reached.Add(Walk(start, vc => vc.ChildViewControllerForHomeIndicatorAutoHidden));
				}
				catch (Exception ex)
				{
					error = ex;
				}
			}

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(container), _ =>
			{
				Assert.True(error is null, error?.ToString());
				var renderer = Assert.IsType<NavigationRenderer>(navPage.Handler);
				Assert.NotEmpty(reached);
				Assert.All(reached, vc => Assert.Same(renderer, vc));
				Assert.Same(((IPlatformViewHandler)root.Handler).ViewController, renderer.ChildViewControllerForStatusBarHidden());
			});

			// Follows the child chain the way UIKit does and returns the controller that answers for itself.
			static UIViewController Walk(UIViewController vc, Func<UIViewController, UIViewController> child)
			{
				for (var next = child(vc); next is not null; next = child(vc))
					vc = next;
				return vc;
			}
		}

		void SetupNavigationRenderer()
		{
			// Renderer-disposal tests must retain their native implementation in inherited handler fixtures.
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler(typeof(FlyoutPage), typeof(PhoneFlyoutPageRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					RegisterCommonHandlers(handlers);
				});
			});
		}

		async Task<(NavigationRenderer, NavigationContinuationContext, ContentPage, UIViewController)> PopAndDismissModalAsync(ContentPage root, bool popToRoot)
		{
			var modal = new NavigationPage(new ContentPage());
			await root.Navigation.PushModalAsync(modal, false);
			await modal.PushAsync(new ContentPage(), false);
			var poppedPage = new ContentPage { Title = "Retained popped page" };
			poppedPage.ToolbarItems.Add(new ToolbarItem { Text = "Retained toolbar item" });
			await modal.PushAsync(poppedPage, false);
			await OnNavigatedToAsync(modal.CurrentPage);

			var renderer = (NavigationRenderer)modal.Handler;
			var poppedController = renderer.TopViewController;
			var original = SynchronizationContext.Current;
			Assert.NotNull(original);
			var context = new NavigationContinuationContext(original);
			try
			{
				// Capture only the continuation scheduled by native pop; do not
				// block the main context needed to dismiss the modal.
				SynchronizationContext.SetSynchronizationContext(context);
				try
				{
					if (popToRoot)
						renderer.PopToRootViewController(false);
					else
						renderer.PopViewController(false);
				}
				finally
				{
					SynchronizationContext.SetSynchronizationContext(original);
				}
				Assert.DoesNotContain(renderer.ViewControllers, controller => controller.Handle == poppedController.Handle);
				await root.Navigation.PopModalAsync(false);
				return (renderer, context, poppedPage, poppedController);
			}
			catch
			{
				context.Release();
				throw;
			}
		}

		sealed class NavigationContinuationContext(SynchronizationContext original) : SynchronizationContext
		{
			readonly ConcurrentQueue<(SendOrPostCallback Callback, object State)> _pending = new();
			readonly ConcurrentQueue<Task> _tasks = new();
			public Task[] Tasks => _tasks.ToArray();

			public override void Post(SendOrPostCallback callback, object state)
			{
				if (state is Task task)
					_tasks.Enqueue(task);
				_pending.Enqueue((callback, state));
			}

			public void Release()
			{
				while (_pending.TryDequeue(out var item))
					original.Post(item.Callback, item.State);
			}
		}
		static bool NativeShouldPopItem(UINavigationController controller) =>
			SendShouldPopItem(controller.Handle, Selector.GetHandle("navigationBar:shouldPopItem:"),
				controller.NavigationBar.Handle, controller.NavigationBar.TopItem.Handle);

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool SendShouldPopItem(IntPtr receiver, IntPtr selector, IntPtr navigationBar, IntPtr item);

		sealed class BackHandlingPage : ContentPage
		{
			public BackHandlingPage(bool backHandled)
			{
				BackHandled = backHandled;
			}

			public bool BackHandled { get; set; }

			public int BackButtonPressedCount { get; private set; }

			protected override bool OnBackButtonPressed()
			{
				BackButtonPressedCount++;
				return BackHandled;
			}
		}
	}
}
