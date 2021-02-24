using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting.Internal
{
	internal class ServiceFactoryAdapter<TContainerBuilder> : IServiceFactoryAdapter
	{
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
		private IServiceProviderFactory<TContainerBuilder>? _serviceProviderFactory;
		private readonly Func<HostBuilderContext>? _contextResolver;
		private readonly Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>>? _factoryResolver;

		public ServiceFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
		{
			_serviceProviderFactory = serviceProviderFactory ?? throw new ArgumentNullException(nameof(serviceProviderFactory));
		}

		public ServiceFactoryAdapter(Func<HostBuilderContext> contextResolver, Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factoryResolver)
		{
			_contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
			_factoryResolver = factoryResolver ?? throw new ArgumentNullException(nameof(factoryResolver));
		}

		public object CreateBuilder(IServiceCollection services)
		{
			if (_serviceProviderFactory == null)
			{
				_serviceProviderFactory = _factoryResolver!(_contextResolver!());

				if (_serviceProviderFactory == null)
				{
					throw new InvalidOperationException("The resolver returned a null IServiceProviderFactory");
				}
			}

			return _serviceProviderFactory.CreateBuilder(services)!;
		}

		public IServiceProvider CreateServiceProvider(object containerBuilder)
		{
			if (_serviceProviderFactory == null)
			{
				throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider");
			}

			return _serviceProviderFactory.CreateServiceProvider((TContainerBuilder)containerBuilder);
		}
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
	}
}
