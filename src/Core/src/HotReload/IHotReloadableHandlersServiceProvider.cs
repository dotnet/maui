#nullable enable
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public interface IHotReloadableHandlersServiceProvider : IMauiHandlersServiceProvider
	{
		IMauiHandlersCollection GetCollection();
	}
}