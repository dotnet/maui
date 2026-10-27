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
				var navController = Assert.IsAssignableFrom<UINavigationController>((navPage.Handler as IPlatformViewHandler)?.ViewController);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.Equal(expectedShouldBegin, recognizer.Delegate.ShouldBegin(recognizer));
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				if (expectedShouldBegin)
				{
					Assert.True(ShouldPopItem(navController));
				}
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				await Task.CompletedTask;
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
				var navController = Assert.IsAssignableFrom<UINavigationController>((navPage.Handler as IPlatformViewHandler)?.ViewController);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.Equal(expectedShouldBegin, recognizer.Delegate.ShouldBegin(recognizer));
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				if (expectedShouldBegin)
				{
					Assert.True(ShouldPopItem(navController));
				}
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				await Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task InteractivePopReturningToCurrentPageReevaluatesBackButtonPolicy(bool navigationBarHidden)
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var backHandlingPage = new BackHandlingPage(false);
			NavigationPage.SetHasNavigationBar(backHandlingPage, !navigationBarHidden);
			var navPage = new NavigationPage(rootPage);
			await navPage.PushAsync(backHandlingPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnNavigatedToAsync(backHandlingPage);
				var navController = Assert.IsAssignableFrom<UINavigationController>((navPage.Handler as IPlatformViewHandler)?.ViewController);
				var recognizer = navController.InteractivePopGestureRecognizer;

				Assert.True(recognizer.Delegate.ShouldBegin(recognizer));
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				Assert.True(ShouldPopItem(navController));
				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);

				// A cancelled pop reports the original top controller again.
				navController.Delegate.DidShowViewController(navController, navController.TopViewController, true);
				backHandlingPage.BackHandled = true;

				Assert.False(ShouldPopItem(navController));
				Assert.Equal(2, backHandlingPage.BackButtonPressedCount);
				Assert.Same(backHandlingPage, navPage.CurrentPage);
				Assert.Equal(2, navController.ViewControllers.Length);
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task NativeBackButtonRespectsBackButtonPressed(bool backHandled)
		{
			SetupBuilder();
			var rootPage = new ContentPage() { Title = "Root Page" };
			var backHandlingPage = new BackHandlingPage(backHandled) { Title = "Second Page" };
			var navPage = new NavigationPage(rootPage);
			await navPage.PushAsync(backHandlingPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async handler =>
			{
				await OnNavigatedToAsync(backHandlingPage);
				var navController = Assert.IsAssignableFrom<UINavigationController>((navPage.Handler as IPlatformViewHandler)?.ViewController);

				navController.NavigationBar.TapBackButton();
				await AssertHelpers.AssertEventually(() => backHandlingPage.BackButtonPressedCount != 0);
				if (!backHandled)
				{
					await OnNavigatedToAsync(rootPage, TimeSpan.FromSeconds(5));
				}

				Assert.Equal(1, backHandlingPage.BackButtonPressedCount);
				Assert.Same(backHandled ? backHandlingPage : rootPage, navPage.CurrentPage);
				Assert.Equal(backHandled ? 2 : 1, navController.ViewControllers.Length);
			});
		}

		static bool ShouldPopItem(UINavigationController navigationController) =>
			SendShouldPopItem(navigationController.Handle, Selector.GetHandle("navigationBar:shouldPopItem:"),
				navigationController.NavigationBar.Handle, navigationController.NavigationBar.TopItem.Handle);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		[return: MarshalAs(UnmanagedType.I1)]
		static extern bool SendShouldPopItem(IntPtr receiver, IntPtr selector, IntPtr navigationBar, IntPtr item);

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
			// This test targets NavigationRenderer.Dispose() specifically, so it always
			// registers NavigationRenderer directly rather than deferring to SetupBuilder()
			// (which, on the Handler subclass, would register NavigationViewHandler instead).
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
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
			// This exercises renderer-specific native-pop continuations, including in handler-derived fixtures.
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					RegisterCommonHandlers(handlers);
				});
			});
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
