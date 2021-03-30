using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiServiceBuilder
	{
		void ConfigureServices(IServiceCollection services);

		void Configure(IServiceProvider services);
	}
}