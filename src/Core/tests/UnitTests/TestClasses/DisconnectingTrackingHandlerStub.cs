using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.UnitTests
{
	// Helper used by the regression tests for https://github.com/dotnet/maui/issues/27101.
	// During DisconnectHandler we trigger an UpdateValue / Invoke call to mimic the cascade caused
	// on Windows by UpdateIsFocused(false) -> ChangeVisualState -> VSM Setters -> mapper. We then
	// assert the mapper was NOT invoked while the handler was tearing down.
	public class DisconnectingTrackingHandlerStub : ViewHandler<Maui.Controls.Button, object>
	{
		const string TestCommand = "DisconnectingTrackingHandlerStub.TestCommand";

		readonly Action _onDisconnect;

		public int MapBackgroundCallCount { get; set; }

		public int InvokeCommandCallCount { get; set; }

		public DisconnectingTrackingHandlerStub(Action onDisconnect)
			: base(BuildMapper(), BuildCommandMapper())
		{
			_onDisconnect = onDisconnect;
		}

		public void InvokeTestCommand() => Invoke(TestCommand, null);

		static IPropertyMapper<IView, IViewHandler> BuildMapper()
		{
			var mapper = new PropertyMapper<IView, IViewHandler>();
			mapper.Add(nameof(IView.Background), (handler, view) =>
			{
				if (handler is DisconnectingTrackingHandlerStub stub)
				{
					stub.MapBackgroundCallCount++;
				}
			});
			return mapper;
		}

		static CommandMapper<IView, IViewHandler> BuildCommandMapper()
		{
			var mapper = new CommandMapper<IView, IViewHandler>();
			mapper.Add(TestCommand, (handler, view, args) =>
			{
				if (handler is DisconnectingTrackingHandlerStub stub)
				{
					stub.InvokeCommandCallCount++;
				}
			});
			return mapper;
		}

		protected override object CreatePlatformView() => new object();

		protected override void DisconnectHandler(object platformView)
		{
			// Call base first to preserve the real disconnect ordering being tested:
			// IElementHandler.DisconnectHandler has already nulled PlatformView, but VirtualView
			// is still wired up while platform teardown runs. For this stub the base is a no-op
			// (ViewHandler<Button, object> has no platform-specific teardown to perform), so
			// invoking _onDisconnect after it still reproduces the mapper cascade that happens
			// during Windows teardown — the handler is in the Disconnecting state at this point.
			base.DisconnectHandler(platformView);
			_onDisconnect?.Invoke();
		}
	}
}
