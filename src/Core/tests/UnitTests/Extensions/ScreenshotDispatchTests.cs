#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Media;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Extensions
{
	[System.ComponentModel.Category(TestCategory.Extensions)]
	public class ScreenshotDispatchTests
	{
		// The test project targets $(_MauiDotNetTfm) = net10.0, so the #else
		// branches of ViewExtensions.CaptureAsync / WindowExtensions.CaptureAsync
		// are what gets compiled here. That's exactly the code path third-party
		// platform backends hit, so these tests validate the real dispatch logic.

		sealed class FakeScreenshotResult : IScreenshotResult
		{
			public int Width => 0;
			public int Height => 0;
			public Task<System.IO.Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
				=> throw new NotImplementedException();
			public Task CopyToAsync(System.IO.Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
				=> throw new NotImplementedException();
		}

		static (IView view, object platformView) CreateViewWithHandler(IServiceProvider services)
		{
			var platformView = new object();
			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(platformView);

			var context = Substitute.For<IMauiContext>();
			context.Services.Returns(services);
			handler.MauiContext.Returns(context);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			return (view, platformView);
		}

		static (IWindow window, object platformView) CreateWindowWithHandler(IServiceProvider services)
		{
			var platformView = new object();
			var handler = Substitute.For<IElementHandler>();
			handler.PlatformView.Returns(platformView);

			var context = Substitute.For<IMauiContext>();
			context.Services.Returns(services);
			handler.MauiContext.Returns(context);

			var window = Substitute.For<IWindow>();
			window.Handler.Returns(handler);

			return (window, platformView);
		}

		[Fact]
		public async Task ViewCaptureAsync_NullView_ReturnsNull()
		{
			IView? view = null;
			var result = await view!.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewCaptureAsync_NoHandler_ReturnsNull()
		{
			var view = Substitute.For<IView>();
			view.Handler.Returns((IViewHandler?)null);

			var result = await view.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewCaptureAsync_NoPlatformView_ReturnsNull()
		{
			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns((object?)null);
			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewCaptureAsync_NoKeyedHookRegistered_ReturnsNull()
		{
			var services = new ServiceCollection().BuildServiceProvider();
			var (view, _) = CreateViewWithHandler(services);

			var result = await view.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewCaptureAsync_KeyedHookRegistered_IsInvokedWithPlatformView()
		{
			object? captured = null;
			var expected = new FakeScreenshotResult();

			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.ViewCapture",
				(_, _) => pv =>
				{
					captured = pv;
					return Task.FromResult<IScreenshotResult?>(expected);
				});

			var (view, platformView) = CreateViewWithHandler(services.BuildServiceProvider());

			var result = await view.CaptureAsync();

			Assert.Same(platformView, captured);
			Assert.Same(expected, result);
		}

		[Fact]
		public async Task ViewCaptureAsync_HookRegisteredUnderWrongKey_ReturnsNull()
		{
			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.WindowCapture", // wrong key for a view
				(_, _) => _ => Task.FromResult<IScreenshotResult?>(new FakeScreenshotResult()));

			var (view, _) = CreateViewWithHandler(services.BuildServiceProvider());

			var result = await view.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewCaptureAsync_HookReturnsNullResult_ReturnsNull()
		{
			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.ViewCapture",
				(_, _) => _ => Task.FromResult<IScreenshotResult?>(null));

			var (view, _) = CreateViewWithHandler(services.BuildServiceProvider());

			var result = await view.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewCaptureAsync_HookReturnsNullTask_ReturnsNull()
		{
			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.ViewCapture",
				(_, _) => _ => null!);

			var (view, _) = CreateViewWithHandler(services.BuildServiceProvider());

			var result = await view.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task WindowCaptureAsync_NullWindow_ReturnsNull()
		{
			IWindow? window = null;
			var result = await window!.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task WindowCaptureAsync_KeyedHookRegistered_IsInvokedWithPlatformView()
		{
			object? captured = null;
			var expected = new FakeScreenshotResult();

			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.WindowCapture",
				(_, _) => pv =>
				{
					captured = pv;
					return Task.FromResult<IScreenshotResult?>(expected);
				});

			var (window, platformView) = CreateWindowWithHandler(services.BuildServiceProvider());

			var result = await window.CaptureAsync();

			Assert.Same(platformView, captured);
			Assert.Same(expected, result);
		}

		[Fact]
		public async Task WindowCaptureAsync_HookRegisteredUnderWrongKey_ReturnsNull()
		{
			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.ViewCapture", // wrong key for a window
				(_, _) => _ => Task.FromResult<IScreenshotResult?>(new FakeScreenshotResult()));

			var (window, _) = CreateWindowWithHandler(services.BuildServiceProvider());

			var result = await window.CaptureAsync();
			Assert.Null(result);
		}

		[Fact]
		public async Task ViewAndWindowHooks_CoexistWithoutInterference()
		{
			var viewResult = new FakeScreenshotResult();
			var windowResult = new FakeScreenshotResult();

			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.ViewCapture",
				(_, _) => _ => Task.FromResult<IScreenshotResult?>(viewResult));
			services.AddKeyedSingleton<Func<object, Task<IScreenshotResult?>>>(
				"Microsoft.Maui.WindowCapture",
				(_, _) => _ => Task.FromResult<IScreenshotResult?>(windowResult));
			var provider = services.BuildServiceProvider();

			var (view, _) = CreateViewWithHandler(provider);
			var (window, _) = CreateWindowWithHandler(provider);

			Assert.Same(viewResult, await view.CaptureAsync());
			Assert.Same(windowResult, await window.CaptureAsync());
		}
	}
}
