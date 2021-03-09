using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	internal interface IServiceFactoryAdapter
	{
		object CreateBuilder(IServiceCollection services);

		IServiceProvider CreateServiceProvider(object containerBuilder);
	}
}