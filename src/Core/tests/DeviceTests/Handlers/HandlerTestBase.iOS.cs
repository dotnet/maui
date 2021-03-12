using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler>
	{
		protected THandler CreateHandler(IView view)
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetVirtualView(view);
			view.Handler = handler;

			return handler;
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityIdentifier;
	}
}