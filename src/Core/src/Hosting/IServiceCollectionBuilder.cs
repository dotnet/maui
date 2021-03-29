using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public interface IServiceCollectionBuilder
	{
		void Build(IServiceCollection services);
	}
}