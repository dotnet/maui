using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowHandlerTests : HandlerTestBase
	{
		//TODO: Fix this test on Android, it fails a lot of times
#if !ANDROID
		[Fact]
		public async Task WindowHasReasonableDisplayDensity()
		{
			var handler = new WindowHandlerProxyStub(
				commandMapper: new()
				{
					[nameof(IWindow.RequestDisplayDensity)] = WindowHandler.MapRequestDisplayDensity
				});

			InitializeViewHandler(new WindowStub(), handler);

			var req = new DisplayDensityRequest();

			var density = await InvokeOnMainThreadAsync(() => handler.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), req));

			Assert.Equal(density, req.Result);
			Assert.InRange(density, 0.1f, 4f);
		}
#endif

#if MACCATALYST || WINDOWS

		[Fact(
#if MACCATALYST
			Skip = "Setting Location on MacCatalyst is currently not supported"
#endif
			)]

		public async Task InitialPositionsAreTakenIntoAccount()
		{
			var window = new Window(new NavigationPage(new ContentPage()))
			{
				Width = 200,
				Height = 500,
				X = 0,
				Y = 500
			};

			await RunWindowTest(window, async handler =>
			{
				// Just let things settle for good measure
				await Task.Delay(100);
				Assert.Equal(200, window.Width);
				Assert.Equal(500, window.Height);
				Assert.Equal(0, window.X);
				Assert.Equal(500, window.Y);
			});
		}

		[Fact]
		public async Task WindowSupportsEmptyPage()
		{
			var window = new Window(new ContentPage());

			await RunWindowTest(window, handler =>
			{
				Assert.NotNull(handler.PlatformView);
			});
		}

		[Theory]
		[InlineData(150, 300)]
		[InlineData(200, 300)]
		[InlineData(500, 500)]
		[InlineData(1000, 1000)]
		public async Task MinimumWidthUpdateWindowSize(double min, double expected)
		{
			const double initial = 300;

			var window = new Window(new ContentPage());

			await RunWindowTest(window, async handler =>
			{
				var platform = handler.PlatformView;

				MovePlatformWindow(platform, new Rect(0, 0, initial, 300));

				Assert.Equal(initial, window.Width);

				window.MinimumWidth = min;

				await Task.Delay(100); // mac catalyst seems to have delays

				Assert.Equal(expected, window.Width);
			});
		}

		[Theory]
		[InlineData(150, 300)]
		[InlineData(200, 300)]
		[InlineData(500, 500)]
		[InlineData(1000, 1000)]
		public async Task MinimumHeightUpdateWindowSize(double min, double expected)
		{
			const double initial = 300;

			var window = new Window(new ContentPage());

			await RunWindowTest(window, async handler =>
			{
				var platform = handler.PlatformView;

				MovePlatformWindow(platform, new Rect(0, 0, initial, 300));

				Assert.Equal(initial, window.Height);

				window.MinimumHeight = min;

				await Task.Delay(100); // mac catalyst seems to have delays

				Assert.Equal(expected, window.Height);
			});
		}

		[Theory]
		[InlineData(150, 150)]
		[InlineData(200, 200)]
		[InlineData(500, 300)]
		[InlineData(1000, 300)]
		public async Task MaximumWidthUpdateWindowSize(double max, double expected)
		{
			const double initial = 300;

			var window = new Window(new ContentPage());

			await RunWindowTest(window, async handler =>
			{
				var platform = handler.PlatformView;

				MovePlatformWindow(platform, new Rect(0, 0, 300, initial));

				Assert.Equal(initial, window.Width);

				window.MaximumWidth = max;

				await Task.Delay(100); // mac catalyst seems to have delays

				Assert.Equal(expected, window.Width);
			});
		}

		[Theory]
		[InlineData(150, 150)]
		[InlineData(200, 200)]
		[InlineData(500, 300)]
		[InlineData(1000, 300)]
		public async Task MaximumHeightUpdateWindowSize(double max, double expected)
		{
			const double initial = 300;

			var window = new Window(new ContentPage());

			await RunWindowTest(window, async handler =>
			{
				var platform = handler.PlatformView;

				MovePlatformWindow(platform, new Rect(0, 0, 300, initial));

				Assert.Equal(initial, window.Height);

				window.MaximumHeight = max;

				await Task.Delay(100); // mac catalyst seems to have delays

				Assert.Equal(expected, window.Height);
			});
		}

		Task RunWindowTest(Window window, Func<IWindowHandler, Task> action)
		{
			var created = new TaskCompletionSource();
			var activated = new TaskCompletionSource();
			var destroying = new TaskCompletionSource();

			window.Created += OnWindowCreated;
			window.Activated += OnWindowActivated;
			window.Destroying += OnWindowDestroying;

			var app = Application.Current;

			return InvokeOnMainThreadAsync(async () =>
			{
				app.OpenWindow(window);

				await created.Task;
				await Task.WhenAny(activated.Task, Task.Delay(3000));

				var windowHandler = window.Handler as IWindowHandler;
				var platformWindow = windowHandler.PlatformView;

#if MACCATALYST
				var retry = 5;
				while (!platformWindow.HasNSWindow() && retry-- > 0)
				{
					await Task.Delay(100);
				}
#endif

				try
				{
					await action(windowHandler);
				}
				finally
				{
					app.CloseWindow(window);

					await destroying.Task;
				}
			});

			void OnWindowCreated(object sender, EventArgs e) =>
				created.TrySetResult();

			void OnWindowActivated(object sender, EventArgs e) =>
				activated.TrySetResult();

			void OnWindowDestroying(object sender, EventArgs e) =>
				destroying.TrySetResult();
		}

		Task RunWindowTest(Window window, Action<IWindowHandler> action) =>
			RunWindowTest(window, handler =>
			{
				action(handler);
				return Task.CompletedTask;
			});

#endif
	}
}