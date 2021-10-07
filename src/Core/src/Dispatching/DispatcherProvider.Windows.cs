using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Dispatching
{
	public class DispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetDispatcher(object context)
		{
			if (context is DependencyObject depObj)
				return new Dispatcher(depObj.DispatcherQueue);

			if (context is Window window)
				return new Dispatcher(window.DispatcherQueue);

			return new Dispatcher(DispatcherQueue.GetForCurrentThread());
		}
	}
}