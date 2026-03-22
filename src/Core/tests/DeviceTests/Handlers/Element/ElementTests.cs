using System;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Element)]
	public partial class ElementTests : CoreHandlerTestBase
	{
		[Fact]
		public void ElementToHandlerReturnsIElementHandler()
		{
			// ElementStub registered in ConfigureTestBuilder
			var handler = new ElementStub().ToHandler(MauiContext);
			Assert.NotNull(handler);
			Assert.IsType<ElementHandlerStub>(handler);
		}

		[Fact]
		public void ElementToHandlerThrowsWhenMatchingHandlerServiceTypeNotRegistered()
		{
			// UnregisteredElementStub not registered in ConfigureTestBuilder
			Assert.Throws<HandlerNotFoundException>(() => new UnregisteredElementStub().ToHandler(MauiContext));
		}

		class UnregisteredElementStub : IElement
		{
			public IElement Parent { get; set; }

			public IElementHandler Handler { get; set; }
		}

		[Fact]
		public void ElementToHandlerPropagatesThrownException()
		{
			var mauiApp = MauiApp.CreateBuilder()
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
			// SetVirtualView is called after grabbing a virtual view's handler via ToHandler
			// Kickoff another virtual view's ToHandler during which an exception is thrown
			public override void SetVirtualView(IElement view)
			{
				new ExceptionThrowingViewStub().ToHandler(MauiContext);
			}
		}

		class ExceptionThrowingViewStub : ElementStub { }
		class ExceptionThrowingViewHandler : ElementHandlerStub
		{
			// SetVirtualView is called after grabbing a virtual view's handler via ToHandler
			// ViewHandlerOfT's virtual method that is called after grabbing the handler service
			public override void SetVirtualView(IElement view)
			{
				throw new HandlerPropagatesException();
			}
		}

		class HandlerPropagatesException : Exception { }
	}
}
