using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GestureManagerTests : BaseTestFixture
	{
		[Fact]
		public void ConnectsWithWindowAndHandlerSet()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();

			view.Window.Returns(new Window());
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);
			Assert.True(gestureManager.IsConnected);
		}

		[Fact]
		public void DoesntConnectWithOnlyWindowSet()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();

			view.Window.Returns(new Window());

			GestureManager gestureManager = new GestureManager(view);
			Assert.True(gestureManager.IsConnected);
		}

		[Fact]
		public void ConnectsWithOnlyHandlerSet()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();

			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);
			Assert.True(gestureManager.IsConnected);
		}

		[Fact]
		public void DisconnectsWhenWindowIsRemoved()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();

			view.Window.Returns(new Window());
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);
			view.Window.Returns((Window)null);
			view.WindowChanged += Raise.Event<EventHandler>(view, EventArgs.Empty);

			Assert.False(gestureManager.IsConnected);
		}

		[Fact]
		public void DisconnectsWhenHandlerIsRemoved()
		{
			var view = Substitute.For<IControlsView>();

			view.Window.Returns(new Window());
			view.Handler.Returns(Substitute.For<IViewHandler>());

			GestureManager gestureManager = new GestureManager(view);
			view.Handler.Returns((IViewHandler)null);
			view.HandlerChanged += Raise.Event<EventHandler>(view, EventArgs.Empty);

			Assert.False(gestureManager.IsConnected);
		}

		[Fact]
		public void PlatformManagerChangesWhenHandlerChanged()
		{
			var view = Substitute.For<IControlsView>();

			view.Window.Returns(new Window());
			view.Handler.Returns(Substitute.For<IViewHandler>());
			GestureManager gestureManager = new GestureManager(view);
			var platformManagerForHandler1 = gestureManager.GesturePlatformManager;
			view.Handler.Returns(Substitute.For<IViewHandler>());
			view.HandlerChanged += Raise.Event<EventHandler>(view, EventArgs.Empty);

			Assert.NotEqual(gestureManager.GesturePlatformManager, platformManagerForHandler1);
		}

		[Fact]
		public void PlatformManagerChangesWhenContainerViewChanged()
		{
			var view = Substitute.For<IControlsView>();
			var handler = Substitute.For<IViewHandler>();

			handler.ContainerView.Returns(null);
			view.Window.Returns(new Window());
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);
			var platformManagerForHandler1 = gestureManager.GesturePlatformManager;

			handler.ContainerView.Returns(new object());
			view.PlatformContainerViewChanged += Raise.Event<EventHandler>(view, EventArgs.Empty);

			Assert.NotEqual(gestureManager.GesturePlatformManager, platformManagerForHandler1);
		}

		[Fact]
		public void UsesDefaultGesturePlatformManagerWhenNoServiceRegistered()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();
			var mauiContext = Substitute.For<IMauiContext>();
			var services = new ServiceCollection().BuildServiceProvider();

			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.NotNull(gestureManager.GesturePlatformManager);
			Assert.IsType<GesturePlatformManager>(gestureManager.GesturePlatformManager);
		}

		[Fact]
		public void UsesCustomGesturePlatformManagerWhenFactoryRegistered()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();
			var mauiContext = Substitute.For<IMauiContext>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();
			var customManager = Substitute.For<IGesturePlatformManager>();
			var services = new ServiceCollection()
				.AddSingleton(factory)
				.BuildServiceProvider();

			factory.CreateGesturePlatformManager(handler).Returns(customManager);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.NotNull(gestureManager.GesturePlatformManager);
			Assert.Equal(customManager, gestureManager.GesturePlatformManager);
			factory.Received(1).CreateGesturePlatformManager(handler);
		}

		[Fact]
		public void FactoryProvidedManagerIsDisposedOnDisconnect()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();
			var mauiContext = Substitute.For<IMauiContext>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();
			var customManager = Substitute.For<IGesturePlatformManager>();
			var services = new ServiceCollection()
				.AddSingleton(factory)
				.BuildServiceProvider();

			factory.CreateGesturePlatformManager(handler).Returns(customManager);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			// Simulate a handler disconnect (e.g., navigation)
			view.Handler.Returns((IViewHandler)null);
			view.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(view, new HandlerChangingEventArgs(handler, null));

			customManager.Received(1).Dispose();
		}

		[Fact]
		public void FactoryCreatesNewManagerAfterReconnect()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();
			var mauiContext = Substitute.For<IMauiContext>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();
			var customManager = Substitute.For<IGesturePlatformManager>();
			var customManager2 = Substitute.For<IGesturePlatformManager>();
			var services = new ServiceCollection()
				.AddSingleton(factory)
				.BuildServiceProvider();

			factory.CreateGesturePlatformManager(handler).Returns(customManager);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);
			Assert.Equal(customManager, gestureManager.GesturePlatformManager);

			// Disconnect
			view.Handler.Returns((IViewHandler)null);
			view.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(view, new HandlerChangingEventArgs(handler, null));
			Assert.False(gestureManager.IsConnected);

			// Reconnect with a new handler
			var handler2 = Substitute.For<IViewHandler>();
			handler2.MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager(handler2).Returns(customManager2);
			view.Handler.Returns(handler2);
			view.HandlerChanged += Raise.Event<EventHandler>(view, EventArgs.Empty);

			Assert.Equal(customManager2, gestureManager.GesturePlatformManager);
			customManager.Received(1).Dispose();
			customManager2.DidNotReceive().Dispose();
			factory.Received(1).CreateGesturePlatformManager(handler);
			factory.Received(1).CreateGesturePlatformManager(handler2);
		}
	}
}
