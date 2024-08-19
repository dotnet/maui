using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowHandlerTests : CoreHandlerTestBase
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
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
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

			await RunWindowTest(window, handler =>
			{
#if MACCATALYST
				// these are updated by the OS
				Assert.True(window.Width > 0, $"Expected Width to be >= 0, but was {window.Width}");
				Assert.True(window.Height > 0, $"Expected Height to be >= 0, but was {window.Height}");
				// these are not available from the OS...
				Assert.True(window.X == 0, $"Expected X to be == 0, but was {window.X}");
				Assert.True(window.Y == 0, $"Expected Y to be == 0, but was {window.Y}");
#elif WINDOWS
				Assert.Equal(200, window.Width);
				Assert.Equal(500, window.Height);
				Assert.Equal(0, window.X);
				Assert.Equal(500, window.Y);
#endif
			});
		}

		[Fact(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task UpdatedPositionsAreTakenIntoAccount()
		{
			var window = new Window(new NavigationPage(new ContentPage()));

			await RunWindowTest(window, async handler =>
			{
				var currentFrame = new Rect(window.X, window.Y, window.Width, window.Height);

				window.Width = 200;
				window.Height = 500;
				window.X = 0;
				window.Y = 0;

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

#if MACCATALYST
				// Mac Catalyst does not support this operation, so it should never change
				Assert.Equal(currentFrame, new Rect(window.X, window.Y, window.Width, window.Height));
#elif WINDOWS
				Assert.Equal(200, window.Width);
				Assert.Equal(500, window.Height);
				Assert.Equal(0, window.X);
				Assert.Equal(0, window.Y);
#endif
			});
		}

		[Fact(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task ChangingTitleWhileChangingTitle()
		{
			var window = new Window(new NavigationPage(new ContentPage()))
			{
				Title = "Initial"
			};

			window.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(Window.Title) && window.Title == "Changed")
				{
					window.Title = "Final";
				}
			};

			await RunWindowTest(window, handler =>
			{
				window.Title = "Changed";

				Assert.Equal("Final", window.Title);
			});
		}

		[Fact(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task ChangingSizeWhileChangingSize()
		{
			var window = new Window(new NavigationPage(new ContentPage()))
			{
				Width = 300,
				Height = 500,
				X = 0,
				Y = 500
			};

			window.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(Window.Width) && window.Width == 400)
				{
					window.Width = 250;
				}
			};

			await RunWindowTest(window, async handler =>
			{
				var currentFrame = new Rect(window.X, window.Y, window.Width, window.Height);

				window.Width = 400;

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

#if MACCATALYST
				// Mac Catalyst does not support this operation, so it should never change
				Assert.Equal(currentFrame, new Rect(window.X, window.Y, window.Width, window.Height));
#elif WINDOWS
				Assert.Equal(250, window.Width);
				Assert.Equal(500, window.Height);
				Assert.Equal(0, window.X);
				Assert.Equal(500, window.Y);
#endif
			});
		}

		[Fact(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task WindowSupportsEmptyPage()
		{
			var window = new Window(new ContentPage());

			await RunWindowTest(window, handler =>
			{
				Assert.NotNull(handler.PlatformView);
			});
		}

		[Theory(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
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

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

				Assert.Equal(expected, window.Width);
			});
		}

		[Theory(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
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

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

				Assert.Equal(expected, window.Height);
			});
		}

		[Theory(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
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

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

				Assert.Equal(expected, window.Width);
			});
		}

		[Theory(
#if CI && MACCATALYST
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
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

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

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
#elif WINDOWS
				// If we don't wait for the content to load then the CloseWindow call crashes
				await ((IPlatformViewHandler)window.Page.Handler).PlatformView.OnLoadedAsync();
#endif

				// Just let things settle as some platforms require a few UI cycles to update bounds
				await Task.Delay(100);

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