namespace Microsoft.Maui.Dispatching
{
	public class SingletonDispatcherProvider : IDispatcherProvider
	{
		IDispatcher? _current;

		public IDispatcher GetDispatcher(object context) =>
			_current ??= new Dispatcher();
	}
}