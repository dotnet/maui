using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Media;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Extensions
{
	[System.ComponentModel.Category(TestCategory.Extensions)]
	public class ViewExtensionsCaptureTests
	{
		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenHandlerIsNull()
		{
			var view = Substitute.For<IView>();
			view.Handler.Returns((IViewHandler)null);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenPlatformViewIsNull()
		{
			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns((object)null);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenScreenshotServiceNotRegistered()
		{
			var services = new ServiceCollection().BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns(mauiContext);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenCaptureNotSupported()
		{
			var screenshot = Substitute.For<IScreenshot>();
			screenshot.IsCaptureSupported.Returns(false);

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns(mauiContext);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenScreenshotDoesNotImplementIViewScreenshot()
		{
			var screenshot = Substitute.For<IScreenshot>();
			screenshot.IsCaptureSupported.Returns(true);

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns(mauiContext);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsCaptureResult_WhenViewScreenshotRegistered()
		{
			var expectedResult = Substitute.For<IScreenshotResult>();
			var platformView = new object();

			var screenshot = Substitute.For<IScreenshot, IViewScreenshot>();
			screenshot.IsCaptureSupported.Returns(true);
			((IViewScreenshot)screenshot).CaptureViewAsync(platformView)
				.Returns(Task.FromResult<IScreenshotResult>(expectedResult));

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(platformView);
			handler.MauiContext.Returns(mauiContext);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Same(expectedResult, result);
			await ((IViewScreenshot)screenshot).Received(1).CaptureViewAsync(platformView);
		}

		[Fact]
		public async Task CaptureAsync_Window_ReturnsNull_WhenHandlerIsNull()
		{
			var window = Substitute.For<IWindow>();
			window.Handler.Returns((IElementHandler)null);

			var result = await window.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_Window_ReturnsNull_WhenScreenshotServiceNotRegistered()
		{
			var services = new ServiceCollection().BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IElementHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns(mauiContext);

			var window = Substitute.For<IWindow>();
			window.Handler.Returns(handler);

			var result = await window.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_Window_ReturnsNull_WhenScreenshotDoesNotImplementIViewScreenshot()
		{
			var screenshot = Substitute.For<IScreenshot>();
			screenshot.IsCaptureSupported.Returns(true);

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IElementHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns(mauiContext);

			var window = Substitute.For<IWindow>();
			window.Handler.Returns(handler);

			var result = await window.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_Window_ReturnsCaptureResult_WhenViewScreenshotRegistered()
		{
			var expectedResult = Substitute.For<IScreenshotResult>();
			var platformView = new object();

			var screenshot = Substitute.For<IScreenshot, IViewScreenshot>();
			screenshot.IsCaptureSupported.Returns(true);
			((IViewScreenshot)screenshot).CaptureViewAsync(platformView)
				.Returns(Task.FromResult<IScreenshotResult>(expectedResult));

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IElementHandler>();
			handler.PlatformView.Returns(platformView);
			handler.MauiContext.Returns(mauiContext);

			var window = Substitute.For<IWindow>();
			window.Handler.Returns(handler);

			var result = await window.CaptureAsync();

			Assert.Same(expectedResult, result);
			await ((IViewScreenshot)screenshot).Received(1).CaptureViewAsync(platformView);
		}

		[Fact]
		public async Task CaptureAsync_View_CapturesContainerView_WhenContainerViewPresent()
		{
			// Parity with the #if PLATFORM path (view.ToPlatform()): when the handler has a
			// container view (clip/shadow/border), it must be captured, not the inner PlatformView.
			var expectedResult = Substitute.For<IScreenshotResult>();
			var platformView = new object();
			var containerView = new object();

			var screenshot = Substitute.For<IScreenshot, IViewScreenshot>();
			screenshot.IsCaptureSupported.Returns(true);
			((IViewScreenshot)screenshot).CaptureViewAsync(containerView)
				.Returns(Task.FromResult<IScreenshotResult>(expectedResult));

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(platformView);
			handler.ContainerView.Returns(containerView);
			handler.MauiContext.Returns(mauiContext);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Same(expectedResult, result);
			await ((IViewScreenshot)screenshot).Received(1).CaptureViewAsync(containerView);
			await ((IViewScreenshot)screenshot).DidNotReceive().CaptureViewAsync(platformView);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenMauiContextIsNull()
		{
			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns((IMauiContext)null);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}

		[Fact]
		public async Task CaptureAsync_View_ReturnsNull_WhenViewScreenshotReturnsNull()
		{
			var screenshot = Substitute.For<IScreenshot, IViewScreenshot>();
			screenshot.IsCaptureSupported.Returns(true);
			((IViewScreenshot)screenshot).CaptureViewAsync(Arg.Any<object>())
				.Returns(Task.FromResult<IScreenshotResult>(null));

			var services = new ServiceCollection()
				.AddSingleton(screenshot)
				.BuildServiceProvider();
			var mauiContext = new MauiContext(services);

			var handler = Substitute.For<IViewHandler>();
			handler.PlatformView.Returns(new object());
			handler.MauiContext.Returns(mauiContext);

			var view = Substitute.For<IView>();
			view.Handler.Returns(handler);

			var result = await view.CaptureAsync();

			Assert.Null(result);
		}
	}
}
