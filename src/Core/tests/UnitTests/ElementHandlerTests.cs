using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class ElementHandlerTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DisconnectClearsVirtualViewAndIsIdempotent(bool platformViewReleased)
		{
			var element = new ShellItem();
			var handler = new TrackingElementHandler();
			handler.SetVirtualView(element);

			if (platformViewReleased)
				handler.ReleasePlatformView();

			((IElementHandler)handler).DisconnectHandler();
			((IElementHandler)handler).DisconnectHandler();

			Assert.Null(handler.PlatformView);
			Assert.Null(handler.VirtualView);
			Assert.Null(element.Handler);
			Assert.Equal(ElementHandlerState.Disconnected, ((IElementHandlerStateExhibitor)handler).State);
			Assert.Equal(platformViewReleased
				? new[] { "Connect" }
				: new[] { "Connect", "Disconnecting", "Disconnect" }, handler.Calls);
		}

		[Fact]
		public void ReleasedPlatformDisconnectPreservesReplacementHandler()
		{
			var element = new ShellItem();
			var previousHandler = new TrackingElementHandler();
			previousHandler.SetVirtualView(element);
			previousHandler.ReleasePlatformView();

			var replacementHandler = new TrackingElementHandler();
			replacementHandler.SetVirtualView(element);
			((IElementHandler)previousHandler).DisconnectHandler();

			Assert.Same(replacementHandler, element.Handler);
			Assert.Same(element, replacementHandler.VirtualView);
			Assert.NotNull(replacementHandler.PlatformView);
			Assert.Null(previousHandler.VirtualView);
			Assert.Equal(new[] { "Connect" }, previousHandler.Calls);

			((IElementHandler)replacementHandler).DisconnectHandler();
		}

		[Fact]
		public void ReplacingVirtualViewAfterPlatformReleaseConnectsTheNewPlatformView()
		{
			var handler = new TrackingElementHandler();
			var firstElement = new ShellItem();
			handler.SetVirtualView(firstElement);
			var firstPlatformView = handler.PlatformView;
			handler.ReleasePlatformView();

			var secondElement = new ShellItem();
			handler.SetVirtualView(secondElement);

			Assert.Null(firstElement.Handler);
			Assert.Same(handler, secondElement.Handler);
			Assert.Same(secondElement, handler.VirtualView);
			Assert.NotSame(firstPlatformView, handler.PlatformView);
			Assert.Equal(new[] { "Connect", "Connect" }, handler.Calls);

			((IElementHandler)handler).DisconnectHandler();
		}

		[Fact]
		public void ReleasedPlatformHandlerCanReconnect()
		{
			var handler = new TrackingElementHandler();
			var firstElement = new ShellItem();
			handler.SetVirtualView(firstElement);
			var firstPlatformView = handler.PlatformView;
			handler.ReleasePlatformView();
			((IElementHandler)handler).DisconnectHandler();

			var secondElement = new ShellItem();
			handler.SetVirtualView(secondElement);

			Assert.Null(firstElement.Handler);
			Assert.Same(handler, secondElement.Handler);
			Assert.Same(secondElement, handler.VirtualView);
			Assert.NotSame(firstPlatformView, handler.PlatformView);
			Assert.Equal(new[] { "Connect", "Connect" }, handler.Calls);

			((IElementHandler)handler).DisconnectHandler();
			Assert.Equal(new[] { "Connect", "Connect", "Disconnecting", "Disconnect" }, handler.Calls);
		}

		sealed class TrackingElementHandler : ElementHandler
		{
			object _connectedPlatformView;

			public TrackingElementHandler() : base(ElementMapper)
			{
			}

			public List<string> Calls { get; } = new List<string>();

			public void ReleasePlatformView() => PlatformView = null;

			private protected override object OnCreatePlatformElement() => new object();

			private protected override void OnConnectHandler(object platformView)
			{
				Assert.Same(PlatformView, platformView);
				_connectedPlatformView = platformView;
				Calls.Add("Connect");
			}

			private protected override void OnDisconnecting()
			{
				Assert.Same(_connectedPlatformView, PlatformView);
				Assert.NotNull(VirtualView);
				Calls.Add("Disconnecting");
				base.OnDisconnecting();
			}

			private protected override void OnDisconnectHandler(object platformView)
			{
				Assert.Null(PlatformView);
				Assert.Same(_connectedPlatformView, platformView);
				Assert.NotNull(VirtualView);
				Calls.Add("Disconnect");
				_connectedPlatformView = null;
			}
		}
	}
}
