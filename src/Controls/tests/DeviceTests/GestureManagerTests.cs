#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Gesture)]
	public class GestureManagerTests : ControlsHandlerTestBase
	{
#if IOS || MACCATALYST || WINDOWS
		// Regression coverage for https://github.com/dotnet/maui/issues/35044.
		//
		// This test intentionally lives in the device-test project (rather than Core.UnitTests)
		// because the guard it exercises is compiled only for Apple and Windows, whose
		// GesturePlatformManager implementations require IPlatformViewHandler.
		//
		// Scenario: a custom/third-party backend connects an IControlsView whose handler
		// implements IViewHandler but NOT IPlatformViewHandler, and no
		// IGesturePlatformManagerFactory / IGesturePlatformManagerProvider is registered. The
		// built-in GesturePlatformManager requires an IPlatformViewHandler to reach the platform
		// view, so GestureManager must skip it (returning null) instead of throwing an
		// invalid-cast exception.
		//
		// The test also covers the completed no-op lifecycle:
		//   * a repeated same-handler event (WindowChanged) re-enters SetupGestureManager while the
		//     stub stays connected without re-attempting the optional factory lookup;
		//   * disconnecting is explicitly asserted not to throw while the manager is null;
		//   * reconnecting the same stub performs one fresh optional factory lookup without throwing.
		// The lookup counts encode the per-connection factory-resolution contract.
		[Fact]
		public async Task GestureManagerSkipsBuiltInManagerWhenHandlerIsNotPlatformViewHandler()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var view = new Label();
				var services = new CountingServiceProvider();

				// A handler that satisfies IViewHandler but is not an IPlatformViewHandler and does
				// not provide its own gesture manager. The service provider records optional
				// IGesturePlatformManagerFactory lookups and returns no factory.
				var handler = new NonPlatformViewHandlerStub(new MauiContext(services));
				view.Handler = handler;

				// Initial connect must not throw and performs one optional factory lookup.
				Assert.Equal(1, services.GestureFactoryRequestCount);

				// Repeated same-handler event: raising WindowChanged (via the internal
				// IWindowController.Window setter) while the same non-IPlatformViewHandler stub is
				// still connected must not retry the already-completed no-op setup.
				((IWindowController)view).Window = new Window();

				Assert.Equal(1, services.GestureFactoryRequestCount);

				// Disconnect: clearing the handler raises HandlerChanging, which invokes
				// DisconnectGestures while GesturePlatformManager is null.
				var disconnectException = Record.Exception(() => view.Handler = null);

				Assert.Null(disconnectException);
				Assert.Equal(1, services.GestureFactoryRequestCount);

				// Reconnect the same stub: setup runs again and performs one new optional factory
				// lookup for the new connection.
				view.Handler = handler;

				Assert.Equal(2, services.GestureFactoryRequestCount);
			});
		}

		// Minimal handler that implements IViewHandler but deliberately does NOT implement
		// IPlatformViewHandler (nor IGesturePlatformManagerProvider), mimicking a custom/third-party
		// backend that supplies its own gesture handling.
		class NonPlatformViewHandlerStub : IViewHandler
		{
			public NonPlatformViewHandlerStub(IMauiContext mauiContext)
			{
				MauiContext = mauiContext;
			}

			public bool HasContainer { get => false; set { } }

			public object? ContainerView => null;

			public object? PlatformView => null;

			public IMauiContext? MauiContext { get; private set; }

			IElement? IElementHandler.VirtualView => null;

			IView? IViewHandler.VirtualView => null;

			public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

			public void PlatformArrange(Rect frame) { }

			public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

			public void SetVirtualView(IElement view) { }

			public void UpdateValue(string property) { }

			public void Invoke(string command, object? args = null) { }

			public void DisconnectHandler() { }
		}

		class CountingServiceProvider : IServiceProvider
		{
			public int GestureFactoryRequestCount { get; private set; }

			public object? GetService(Type serviceType)
			{
				if (serviceType == typeof(IGesturePlatformManagerFactory))
					GestureFactoryRequestCount++;

				return null;
			}
		}
#endif
	}
}
