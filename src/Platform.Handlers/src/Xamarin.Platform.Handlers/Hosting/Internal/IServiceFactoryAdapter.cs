using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xamarin.Platform.Hosting.Internal
{
	internal interface IServiceFactoryAdapter
	{
		object CreateBuilder(IServiceCollection services);

		IServiceProvider CreateServiceProvider(object containerBuilder);
	}
}