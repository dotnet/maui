using System;
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
		public void UsesDefaultGesturePlatformManagerWhenHandlerDoesNotProvideCustomManager()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();

			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.NotNull(gestureManager.GesturePlatformManager);
			Assert.IsType<GesturePlatformManager>(gestureManager.GesturePlatformManager);
		}

		[Fact]
		public void UsesCustomGesturePlatformManagerWhenHandlerProvidesOne()
		{
			var handler = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var gesturePlatformManagerProvider = (IGesturePlatformManagerProvider)handler;
			var view = Substitute.For<IControlsView>();
			var customManager = Substitute.For<IGesturePlatformManager>();

			gesturePlatformManagerProvider.CreateGesturePlatformManager().Returns(customManager);
			view.Handler.Returns((IViewHandler)handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.NotNull(gestureManager.GesturePlatformManager);
			Assert.Equal(customManager, gestureManager.GesturePlatformManager);
			gesturePlatformManagerProvider.Received(1).CreateGesturePlatformManager();
		}

		[Fact]
		public void ThrowsWhenHandlerProvidedManagerIsNull()
		{
			var handler = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var gesturePlatformManagerProvider = (IGesturePlatformManagerProvider)handler;
			var view = Substitute.For<IControlsView>();

			gesturePlatformManagerProvider.CreateGesturePlatformManager().Returns((IGesturePlatformManager)null);
			view.Handler.Returns((IViewHandler)handler);

			Assert.Throws<InvalidOperationException>(() => new GestureManager(view));
			gesturePlatformManagerProvider.Received(1).CreateGesturePlatformManager();
		}

		[Fact]
		public void HandlerProvidedManagerIsDisposedOnDisconnect()
		{
			var handler = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var gesturePlatformManagerProvider = (IGesturePlatformManagerProvider)handler;
			var view = Substitute.For<IControlsView>();
			var customManager = Substitute.For<IGesturePlatformManager>();

			gesturePlatformManagerProvider.CreateGesturePlatformManager().Returns(customManager);
			view.Handler.Returns((IViewHandler)handler);

			GestureManager gestureManager = new GestureManager(view);

			view.Handler.Returns((IViewHandler)null);
			view.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(view, new HandlerChangingEventArgs((IViewHandler)handler, null));

			customManager.Received(1).Dispose();
		}

		[Fact]
		public void HandlerProviderCreatesNewManagerAfterReconnect()
		{
			var handler = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var gesturePlatformManagerProvider = (IGesturePlatformManagerProvider)handler;
			var view = Substitute.For<IControlsView>();
			var customManager = Substitute.For<IGesturePlatformManager>();
			var customManager2 = Substitute.For<IGesturePlatformManager>();

			gesturePlatformManagerProvider.CreateGesturePlatformManager().Returns(customManager);
			view.Handler.Returns((IViewHandler)handler);

			GestureManager gestureManager = new GestureManager(view);
			Assert.Equal(customManager, gestureManager.GesturePlatformManager);

			view.Handler.Returns((IViewHandler)null);
			view.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(view, new HandlerChangingEventArgs((IViewHandler)handler, null));
			Assert.False(gestureManager.IsConnected);

			var handler2 = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var gesturePlatformManagerProvider2 = (IGesturePlatformManagerProvider)handler2;
			gesturePlatformManagerProvider2.CreateGesturePlatformManager().Returns(customManager2);
			view.Handler.Returns((IViewHandler)handler2);
			view.HandlerChanged += Raise.Event<EventHandler>(view, EventArgs.Empty);

			Assert.Equal(customManager2, gestureManager.GesturePlatformManager);
			customManager.Received(1).Dispose();
			customManager2.DidNotReceive().Dispose();
			gesturePlatformManagerProvider.Received(1).CreateGesturePlatformManager();
			gesturePlatformManagerProvider2.Received(1).CreateGesturePlatformManager();
		}

		[Fact]
		public void UsesFactoryRegisteredInServicesToCreateManager()
		{
			var customManager = Substitute.For<IGesturePlatformManager>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();

			var handler = Substitute.For<IViewHandler>();
			var mauiContext = Substitute.For<IMauiContext>();
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IGesturePlatformManagerFactory)).Returns(factory);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager(handler).Returns(customManager);

			var view = Substitute.For<IControlsView>();
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.Equal(customManager, gestureManager.GesturePlatformManager);
			factory.Received(1).CreateGesturePlatformManager(handler);

			view.Handler.Returns((IViewHandler)null);
			view.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(view, new HandlerChangingEventArgs(handler, null));
			customManager.Received(1).Dispose();
		}

		[Fact]
		public void ServicesFactoryTakesPrecedenceOverHandlerProvider()
		{
			var factoryManager = Substitute.For<IGesturePlatformManager>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();

			var handler = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var provider = (IGesturePlatformManagerProvider)handler;
			var providerManager = Substitute.For<IGesturePlatformManager>();
			provider.CreateGesturePlatformManager().Returns(providerManager);

			var mauiContext = Substitute.For<IMauiContext>();
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IGesturePlatformManagerFactory)).Returns(factory);
			mauiContext.Services.Returns(services);
			((IViewHandler)handler).MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager((IViewHandler)handler).Returns(factoryManager);

			var view = Substitute.For<IControlsView>();
			view.Handler.Returns((IViewHandler)handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.Equal(factoryManager, gestureManager.GesturePlatformManager);
			provider.DidNotReceive().CreateGesturePlatformManager();
		}

		[Fact]
		public void ThrowsWhenFactoryProvidedManagerIsNull()
		{
			var factory = Substitute.For<IGesturePlatformManagerFactory>();

			var handler = Substitute.For<IViewHandler>();
			var mauiContext = Substitute.For<IMauiContext>();
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IGesturePlatformManagerFactory)).Returns(factory);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager(handler).Returns((IGesturePlatformManager)null);

			var view = Substitute.For<IControlsView>();
			view.Handler.Returns(handler);

			Assert.Throws<InvalidOperationException>(() => new GestureManager(view));
			factory.Received(1).CreateGesturePlatformManager(handler);
		}

		// ── IGesturePlatformManagerFactory public-API extensibility tests ──────────────────

		/// <summary>
		/// When a DI-registered <see cref="IGesturePlatformManagerFactory"/> is present the
		/// factory-supplied manager is returned instead of the built-in
		/// <see cref="GesturePlatformManager"/> type (covers issue #33364).
		/// </summary>
		[Fact]
		public void FactoryManagerIsNotBuiltInGesturePlatformManagerType()
		{
			var customManager = Substitute.For<IGesturePlatformManager>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();

			var handler = Substitute.For<IViewHandler>();
			var mauiContext = Substitute.For<IMauiContext>();
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IGesturePlatformManagerFactory)).Returns(factory);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager(handler).Returns(customManager);

			var view = Substitute.For<IControlsView>();
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.Equal(customManager, gestureManager.GesturePlatformManager);
			// The factory's instance must not be the built-in concrete type.
			Assert.IsNotType<GesturePlatformManager>(gestureManager.GesturePlatformManager);
			factory.Received(1).CreateGesturePlatformManager(handler);
		}

		/// <summary>
		/// When a DI factory is registered and the handler also implements
		/// <see cref="IGesturePlatformManagerProvider"/>, the factory manager is returned and
		/// the handler-scoped provider is not called (covers the precedence guarantee in #33364).
		/// </summary>
		[Fact]
		public void FactoryTakesPrecedenceOverProvider()
		{
			var factoryManager = Substitute.For<IGesturePlatformManager>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();

			var handler = Substitute.For<IViewHandler, IGesturePlatformManagerProvider>();
			var provider = (IGesturePlatformManagerProvider)handler;
			var providerManager = Substitute.For<IGesturePlatformManager>();
			provider.CreateGesturePlatformManager().Returns(providerManager);

			var mauiContext = Substitute.For<IMauiContext>();
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IGesturePlatformManagerFactory)).Returns(factory);
			mauiContext.Services.Returns(services);
			((IViewHandler)handler).MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager((IViewHandler)handler).Returns(factoryManager);

			var view = Substitute.For<IControlsView>();
			view.Handler.Returns((IViewHandler)handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.Equal(factoryManager, gestureManager.GesturePlatformManager);
			Assert.IsNotType<GesturePlatformManager>(gestureManager.GesturePlatformManager);
			// Handler-scoped provider must be completely bypassed when the factory is registered.
			provider.DidNotReceive().CreateGesturePlatformManager();
			factory.Received(1).CreateGesturePlatformManager((IViewHandler)handler);
		}

		/// <summary>
		/// When a DI factory is registered and the handler is not an
		/// <see cref="IPlatformViewHandler"/> (custom/third-party backend), the factory is still
		/// invoked and the returned manager is used — factory registration fully decouples
		/// gesture setup from the built-in platform view contract.
		/// <para>
		/// Note: this only covers the <em>factory-present</em> path. The core #35044 regression —
		/// where no factory/provider is registered and the built-in
		/// <see cref="GesturePlatformManager"/> must be skipped for a non-<see cref="IPlatformViewHandler"/>
		/// handler — lives behind <c>#if PLATFORM</c> and cannot be compiled here (netstandard).
		/// That scenario is covered by
		/// <c>Controls.DeviceTests</c>' <c>GestureManagerTests</c>.
		/// </para>
		/// </summary>
		[Fact]
		public void FactoryUsedEvenWhenHandlerIsNotIPlatformViewHandler()
		{
			var customManager = Substitute.For<IGesturePlatformManager>();
			var factory = Substitute.For<IGesturePlatformManagerFactory>();

			// Plain IViewHandler — does NOT implement IPlatformViewHandler.
			var handler = Substitute.For<IViewHandler>();
			var mauiContext = Substitute.For<IMauiContext>();
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IGesturePlatformManagerFactory)).Returns(factory);
			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			factory.CreateGesturePlatformManager(handler).Returns(customManager);

			var view = Substitute.For<IControlsView>();
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.True(gestureManager.IsConnected);
			Assert.Equal(customManager, gestureManager.GesturePlatformManager);
			factory.Received(1).CreateGesturePlatformManager(handler);
		}
	}
}
