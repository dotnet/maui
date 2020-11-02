using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Xamarin.Platform.Handlers.UnitTests
{

	[TestFixture]
	public class AbstractViewHandlerTests
	{
		[Test]
		public void ConnectAndDisconnectFireAppropriateNumberOfTimes()
		{
			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Forms.Button());

			Assert.AreEqual(1, handlerStub.ConnectHandlerCount);
			Assert.AreEqual(0, handlerStub.DisconnectHandlerCount);

			handlerStub.SetVirtualView(new Forms.Button());
			handlerStub.SetVirtualView(new Forms.Button());
			handlerStub.SetVirtualView(new Forms.Button());
			Assert.AreEqual(1, handlerStub.ConnectHandlerCount);
			Assert.AreEqual(0, handlerStub.DisconnectHandlerCount);

			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.AreEqual(1, handlerStub.ConnectHandlerCount);
			Assert.AreEqual(1, handlerStub.DisconnectHandlerCount);

			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.AreEqual(1, handlerStub.ConnectHandlerCount);
			Assert.AreEqual(1, handlerStub.DisconnectHandlerCount);


			handlerStub.SetVirtualView(new Forms.Button());
			Assert.AreEqual(2, handlerStub.ConnectHandlerCount);
			Assert.AreEqual(1, handlerStub.DisconnectHandlerCount);
			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.AreEqual(2, handlerStub.ConnectHandlerCount);
			Assert.AreEqual(2, handlerStub.DisconnectHandlerCount);
		}
	}

}
