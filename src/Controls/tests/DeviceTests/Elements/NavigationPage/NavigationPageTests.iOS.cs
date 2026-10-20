using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task NavigatingBackViaBackButtonFiresNavigatedEvent()
		{
			SetupBuilder();
			var page = new ContentPage();

			var navPage = new NavigationPage(page) { Title = "App Page" };

			await navPage.PushAsync(new ContentPage());
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(navPage.CurrentPage);
				var navController = navPage.Handler as UINavigationController;

				Assert.False(page.HasNavigatedTo);
				navController.NavigationBar.TapBackButton();
				await OnNavigatedToAsync(page);
				Assert.True(page.HasNavigatedTo);
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
			SetupBuilder();

			var root = new ContentPage()
			{
				Title = "root",
				Content = new Label { Text = "Hello" }
			};

			await root.Dispatcher.DispatchAsync(() =>
			{
				var navPage = new NavigationPage(root);
				var handler = CreateHandler(navPage);

				// Calling Dispose more than once should be fine
				(handler as NavigationRenderer).Dispose();
				(handler as NavigationRenderer).Dispose();
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NativePopContinuationAfterRendererDisposalDoesNotThrow(bool popToRoot)
		{
			SetupBuilder();
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
	}
}
