namespace Microsoft.Maui.Dispatching
{
	public partial class DispatcherProvider : IDispatcherProvider
	{
		public IDispatcher? GetForCurrentThread() =>
			GetForCurrentThreadImplementation();
	}
}