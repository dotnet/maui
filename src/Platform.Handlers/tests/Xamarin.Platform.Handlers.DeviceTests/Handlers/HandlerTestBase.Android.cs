using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xamarin.Platform;
using Android.Widget;
using System.Threading.Tasks;

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
