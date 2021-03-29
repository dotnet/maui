using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	interface IServiceFactoryAdapter
	{
		object CreateBuilder(IServiceCollection services);

		IServiceProvider CreateServiceProvider(object containerBuilder);
	}
}