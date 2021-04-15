using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting
{
	public interface IAppHost : IHost
	{
		IMauiHandlersServiceProvider Handlers { get; }
	}
}