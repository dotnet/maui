using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Widget;
using Xamarin.Platform;
using Xunit;

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
