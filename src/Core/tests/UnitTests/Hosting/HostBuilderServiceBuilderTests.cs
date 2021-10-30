using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderServiceBuilderTests
	{
		[Fact]
		public void MultipleServicesAreRegisteredWithoutBuilder()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton(new MappingService("key 1", "value 1"));
			builder.Services.AddSingleton(new MappingService("key 2", "value 2"));
			var mauiApp = builder.Build();

			var mappingServices = mauiApp.Services.GetServices<MappingService>().ToArray();

			Assert.Equal(2, mappingServices.Length);
			Assert.NotEqual(mappingServices[0], mappingServices[1]);

			Assert.Equal(new[] { "value 1" }, Get("key 1"));
			Assert.Equal(new[] { "value 2" }, Get("key 2"));
			Assert.Empty(Get("key 3"));

			IEnumerable<string> Get(string key)
			{
				foreach (var service in mappingServices)
					foreach (var value in service.Get(key))
						yield return value;
			}
		}

		[Fact]
		public void AppConfigurationReachesBuilder()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Host
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value 1" },
					});
				});

			var appConfigValue = builder.Configuration["key 1"];
			var mauiApp = builder.Build();

			Assert.Equal("value 1", appConfigValue);
		}

		class MicrosoftExtensionsServiceProviderFactory : IServiceProviderFactory<ServiceCollection>
		{
			public ServiceCollection CreateBuilder(IServiceCollection services) =>
				new ServiceCollection { services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder) =>
				containerBuilder.BuildServiceProvider();
		}
	}
}