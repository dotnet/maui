using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiServiceBuilder
	{
		void ConfigureServices(HostBuilderContext context, IServiceCollection services);

		void Configure(HostBuilderContext context, IServiceProvider services);
	}
}