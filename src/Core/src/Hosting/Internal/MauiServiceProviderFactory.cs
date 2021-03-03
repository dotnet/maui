using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	internal class MauiServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
	{
		public IServiceCollection CreateBuilder(IServiceCollection services)
		{
			if (services is IMauiServiceCollection mauiServiceCollection)
				return mauiServiceCollection;

			var collection = new MauiServiceCollection();

			foreach (var item in services)
				collection.Add(item);

			return collection;
		}

		public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
		{
			if (containerBuilder is IMauiServiceCollection mauiServiceCollection)
				return mauiServiceCollection.BuildServiceProvider();
			else
				throw new InvalidCastException($"{nameof(containerBuilder)} is not {nameof(IMauiServiceCollection)}");

		}
	}
}
