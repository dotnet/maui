using System;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class ElementTests
	{
		[Fact]
		public void ElementToHandlerReturnsIElementHandler()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ElementStub, ElementHandlerStub>())
				.Build();
			var context = new MauiContext(app.Services);
			var element = new ElementStub();
			var handler = element.ToHandler(context);
			Assert.NotNull(handler);
			Assert.IsType<ElementHandlerStub>(handler);
			Assert.Same(handler, element.Handler);
			Assert.Same(element, handler.VirtualView);
			Assert.Same(context, handler.MauiContext);
		}

		[Fact]
		public void ElementToHandlerThrowsWhenMatchingHandlerServiceTypeNotRegistered()
		{
			using var app = MauiApp.CreateBuilder().Build();
			var context = new MauiContext(app.Services);
			Assert.Throws<HandlerNotFoundException>(() => new UnregisteredElementStub().ToHandler(context));
		}

		class UnregisteredElementStub : IElement
		{
			public IElement Parent { get; set; }

			public IElementHandler Handler { get; set; }
		}

		[Fact]
		public void ElementToHandlerPropagatesThrownException()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ViewWithExceptionThrowingChildStub, ViewWithExceptionThrowingChildHandler>();
					handlers.AddHandler<ExceptionThrowingViewStub, ExceptionThrowingViewHandler>();
				})
				.Build();

			var mauiContext = new MauiContext(mauiApp.Services);

			Assert.Throws<HandlerPropagatesException>(() =>
				new ViewWithExceptionThrowingChildStub().ToHandler(mauiContext)
			);
		}

		class ViewWithExceptionThrowingChildStub : ElementStub { }
		class ViewWithExceptionThrowingChildHandler : ElementHandlerStub
		{
			public override void SetVirtualView(IElement view)
			{
				// A nested lookup must propagate the original handler initialization exception.
				new ExceptionThrowingViewStub().ToHandler(MauiContext);
			}
		}

		class ExceptionThrowingViewStub : ElementStub { }
		class ExceptionThrowingViewHandler : ElementHandlerStub
		{
			public override void SetVirtualView(IElement view)
			{
				throw new HandlerPropagatesException();
			}
		}

		class HandlerPropagatesException : Exception { }

		class ElementStub : IElement
		{
			public IElement Parent { get; set; }

			public IElementHandler Handler { get; set; }
		}

		class ElementHandlerStub : ElementHandler<ElementStub, object>
		{
			public ElementHandlerStub() : base(ElementHandler.ElementMapper)
			{
			}

			protected override object CreatePlatformElement() => new object();
		}
	}
}
