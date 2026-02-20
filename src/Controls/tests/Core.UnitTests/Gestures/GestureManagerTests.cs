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
		public void UsesCustomGesturePlatformManagerWhenServiceRegistered()
		{
			var handler = Substitute.For<IViewHandler>();
			var view = Substitute.For<IControlsView>();
			var mauiContext = Substitute.For<IMauiContext>();
			var customManager = Substitute.For<IGesturePlatformManager>();

			var services = new ServiceCollection()
				.AddSingleton(customManager)
				.BuildServiceProvider();

			mauiContext.Services.Returns(services);
			handler.MauiContext.Returns(mauiContext);
			view.Handler.Returns(handler);

			GestureManager gestureManager = new GestureManager(view);

			Assert.NotNull(gestureManager.GesturePlatformManager);
			Assert.Equal(customManager, gestureManager.GesturePlatformManager);
		}
	}
}
