using System;
using System.Collections.Generic;
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

		[Fact]
		public void BasicConnectAndDisconnect()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, ViewHandlerWithConnectTracking>())
				.Build();

			var stub = new ViewStub();
			var mauiContext = new MauiContext(mauiApp.Services);
			var handler = stub.ToHandler(mauiContext) as ViewHandlerWithConnectTracking;
			Assert.Empty(handler.DisconnectFrom);
			Assert.Single(handler.ConnectedTo);
			Assert.Contains(stub, handler.ConnectedTo);

			(handler as IElementHandler).Disconnect(false);

			Assert.Single(handler.DisconnectFrom);
			Assert.Single(handler.ConnectedTo);
			Assert.Contains(stub, handler.ConnectedTo);
		}

		[Fact]
		public void SettingNewVirtualViewFiresConnectAndDisconnect()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, ViewHandlerWithConnectTracking>())
				.Build();

			var stub = new ViewStub();
			var stub2 = new ViewStub();

			var mauiContext = new MauiContext(mauiApp.Services);
			var handler = stub.ToHandler(mauiContext) as ViewHandlerWithConnectTracking;

			handler.SetVirtualView(stub2);

			Assert.Contains(stub, handler.ConnectedTo);
			Assert.Contains(stub2, handler.ConnectedTo);
			Assert.Contains(stub, handler.DisconnectFrom);
		}

		[Fact]
		public void DisconnectWontFireTwice()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, ViewHandlerWithConnectTracking>())
				.Build();

			var stub = new ViewStub();
			var mauiContext = new MauiContext(mauiApp.Services);
			var handler = stub.ToHandler(mauiContext) as ViewHandlerWithConnectTracking;

			(handler as IElementHandler).Disconnect(false);
			(handler as IElementHandler).Disconnect(false);

			Assert.Single(handler.DisconnectFrom);
		}

		[Fact]
		public void ConnectWontFireTwice()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, ViewHandlerWithConnectTracking>())
				.Build();

			var stub = new ViewStub();
			var mauiContext = new MauiContext(mauiApp.Services);
			var handler = stub.ToHandler(mauiContext) as ViewHandlerWithConnectTracking;

			(handler as IElementHandler).Connect();
			(handler as IElementHandler).Connect();

			Assert.Single(handler.ConnectedTo);
		}

		class ViewHandlerWithConnectTracking : ViewHandlerStub
		{
			public bool IsDestroyed { get; set; }
			public bool IsConnected { get; set; }

			public List<object> ConnectedTo = new List<object>();
			public List<object> DisconnectFrom = new List<object>();

			protected override void Connect()
			{
				if (IsConnected)
					throw new Exception("Handler has already been connected");

				if (IsDestroyed)
					throw new Exception("Handler has already been destroyed");

				base.Connect();
				ConnectedTo.Add(VirtualView);
				IsConnected = true;
			}

			protected override void Disconnect(bool isDestroying)
			{
				if (IsDestroyed)
					throw new Exception("Handler has already been destroyed");

				IsDestroyed = isDestroying;
				base.Disconnect(isDestroying);
				DisconnectFrom.Add(VirtualView);
				IsConnected = false;
			}
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