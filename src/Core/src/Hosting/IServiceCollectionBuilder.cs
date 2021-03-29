using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public interface IServiceCollectionBuilder
	{
		void Build(IServiceCollection services);

		void Configure(IServiceProvider services);
	}
}