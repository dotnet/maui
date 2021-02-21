using System;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class HandlerTestBase<THandler>
	{
		protected THandler CreateHandler(IView view)
		{
			var handler = Activator.CreateInstance<THandler>();
			if (handler is IAndroidViewHandler av)
				av.SetContext(DefaultContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			return handler;
		}
	}
}
