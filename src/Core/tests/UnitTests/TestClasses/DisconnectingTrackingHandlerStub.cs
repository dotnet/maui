using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.UnitTests
{
	// Helper used by the regression test for https://github.com/dotnet/maui/issues/27101.
	// During DisconnectHandler we trigger an UpdateValue call to mimic the cascade caused on
	// Windows by UpdateIsFocused(false) -> ChangeVisualState -> VSM Setters -> mapper. We then
	// assert the mapper was NOT invoked while the handler was tearing down.
	public class DisconnectingTrackingHandlerStub : ViewHandler<Maui.Controls.Button, object>
	{
		readonly Action _onDisconnect;

		public int MapBackgroundCallCount { get; set; }

		public DisconnectingTrackingHandlerStub(Action onDisconnect)
			: base(BuildMapper())
		{
			_onDisconnect = onDisconnect;
		}

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

		protected override object CreatePlatformView() => new object();

		protected override void DisconnectHandler(object platformView)
		{
			base.DisconnectHandler(platformView);
			_onDisconnect?.Invoke();
		}
	}
}
