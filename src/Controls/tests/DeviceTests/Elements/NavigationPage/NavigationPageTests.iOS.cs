using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

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
