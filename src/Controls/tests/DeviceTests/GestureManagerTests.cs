#nullable enable

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
		// Regression coverage for https://github.com/dotnet/maui/issues/35044.
		//
		// This test intentionally lives in the device-test project (rather than Core.UnitTests)
		// because the guard it exercises is compiled only under `#if PLATFORM`
		// (GestureManager.CreateGesturePlatformManager). The netstandard-based Core.UnitTests
		// never compiles that branch, so the platform behaviour can only be verified here where
		// PLATFORM is defined for every target framework (android/ios/maccatalyst/windows).
		//
		// Scenario: a custom/third-party backend connects an IControlsView whose handler
		// implements IViewHandler but NOT IPlatformViewHandler, and no
		// IGesturePlatformManagerFactory / IGesturePlatformManagerProvider is registered. The
		// built-in GesturePlatformManager requires an IPlatformViewHandler to reach the platform
		// view, so GestureManager must skip it (returning null) instead of throwing an
		// invalid-cast exception. IsConnected must therefore be false.
		//
		// The test also covers null-manager lifecycle safety: when CreateGesturePlatformManager
		// returns null, the GestureManager must survive the ordinary connect/disconnect/reconnect
		// event churn without ever dereferencing the null manager:
		//   * a repeated same-handler event (WindowChanged) re-enters SetupGestureManager while the
		//     stub stays connected — the `_handler is not null` guard means the null-returning
		//     CreateGesturePlatformManager is not re-attempted, and nothing throws;
		//   * clearing the handler makes HandlerChanging call DisconnectGestures with a null manager
		//     (GesturePlatformManager?.Dispose() must be null-safe);
		//   * reconnecting the same stub re-runs setup, which skips the built-in manager again.
		[Fact]
		public async Task GestureManagerSkipsBuiltInManagerWhenHandlerIsNotPlatformViewHandler()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var view = new Label();

				// A handler that satisfies IViewHandler but is not an IPlatformViewHandler and does
				// not provide its own gesture manager (no IGesturePlatformManagerProvider), with no
				// MauiContext/Services so no IGesturePlatformManagerFactory can be resolved.
				var handler = new NonPlatformViewHandlerStub();
				view.Handler = handler;

				// Initial connect: setting up gesture management for this handler must not throw and
				// must not create the built-in platform gesture infrastructure.
				var gestureManager = new GestureManager((IControlsView)view);

				Assert.False(gestureManager.IsConnected);
				Assert.Null(gestureManager.GesturePlatformManager);

				// Repeated same-handler event: raising WindowChanged (via the internal
				// IWindowController.Window setter) while the same non-IPlatformViewHandler stub is
				// still connected re-enters SetupGestureManager. With a null manager this must stay
				// disconnected and must not throw — this is the exact scenario the SetupGestureManager
				// `_handler is not null` early-return guard protects.
				((IWindowController)view).Window = new Window();

				Assert.False(gestureManager.IsConnected);
				Assert.Null(gestureManager.GesturePlatformManager);

				// Disconnect: clearing the handler raises HandlerChanging, which invokes
				// DisconnectGestures while GesturePlatformManager is null. Disposing/clearing a null
				// manager must not throw.
				view.Handler = null;

				Assert.False(gestureManager.IsConnected);
				Assert.Null(gestureManager.GesturePlatformManager);

				// Reconnect the same stub: setup runs again, the built-in manager is skipped again,
				// and the manager stays null without throwing.
				view.Handler = handler;

				Assert.False(gestureManager.IsConnected);
				Assert.Null(gestureManager.GesturePlatformManager);
			});
		}

		// Minimal handler that implements IViewHandler but deliberately does NOT implement
		// IPlatformViewHandler (nor IGesturePlatformManagerProvider), mimicking a custom/third-party
		// backend that supplies its own gesture handling.
		class NonPlatformViewHandlerStub : IViewHandler
		{
			public bool HasContainer { get => false; set { } }

			public object? ContainerView => null;

			public object? PlatformView => null;

			public IMauiContext? MauiContext => null;

			IElement? IElementHandler.VirtualView => null;

			IView? IViewHandler.VirtualView => null;

			public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

			public void PlatformArrange(Rect frame) { }

			public void SetMauiContext(IMauiContext mauiContext) { }

			public void SetVirtualView(IElement view) { }

			public void UpdateValue(string property) { }

			public void Invoke(string command, object? args = null) { }

			public void DisconnectHandler() { }
		}
	}
}
