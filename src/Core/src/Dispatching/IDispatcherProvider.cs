#nullable enable

namespace Microsoft.Maui.Dispatching
{
	public interface IDispatcherProvider
	{
		IDispatcher GetDispatcher(object context);
	}
}