using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core)]
	public class HandlerTests
	{
		[Fact]
		public void HandlerNotFoundExceptionWhenHandlerNotRegistered()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiContext = new MauiContext(mauiApp.Services);

			Assert.Throws<HandlerNotFoundException>(() =>
				new ViewStub().ToPlatform(mauiContext)
			);
		}

		[Fact]
		public void ExceptionPropagatesFromHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerWithChildException>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ExceptionThrowingStub, ExceptionThrowingViewHandler>())
				.Build();

			var mauiContext = new MauiContext(mauiApp.Services);

			Assert.Throws<PlatformViewStubCreatePlatformViewException>(() =>
				new ViewStub().ToPlatform(mauiContext)
			);
		}

		class ViewHandlerWithChildException : ViewHandlerStub
		{
			public ViewHandlerWithChildException()
			{
			}

			protected override PlatformViewStub CreatePlatformView()
			{
				return new PlatformViewStub();
			}

			public override void SetVirtualView(IView view)
			{
				base.SetVirtualView(view);
				new ExceptionThrowingStub().ToPlatform(MauiContext);
			}
		}

		class ExceptionThrowingStub : ViewStub
		{

		}

		class ExceptionThrowingViewHandler : ViewHandlerStub
		{
			public ExceptionThrowingViewHandler()
			{
			}

			protected override PlatformViewStub CreatePlatformView()
			{
				throw new PlatformViewStubCreatePlatformViewException();
			}
		}

		class PlatformViewStubCreatePlatformViewException : Exception
		{

		}
	}
}