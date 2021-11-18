namespace Microsoft.Maui.Dispatching
{
	public interface IDispatcherProvider
	{
		IDispatcher? GetForCurrentThread();
	}
}