using System;

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
	}
}