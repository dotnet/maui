using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Tests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderServiceBuilderTests
	{
		[Fact]
		public void MultipleServicesAreRegisteredWithoutBuilder()
		{
			var host = new AppHostBuilder()
				.UseServiceProviderFactory(new MicrosoftExtensionsServiceProviderFactory())
				.ConfigureServices((context, services) => services.AddSingleton(new MappingService("key 1", "value 1")))
				.ConfigureServices((context, services) => services.AddSingleton(new MappingService("key 2", "value 2")))
				.Build();

			var services = host.Services.GetServices<MappingService>().ToArray();

			Assert.Equal(2, services.Length);
			Assert.NotEqual(services[0], services[1]);

			Assert.Equal(new[] { "value 1" }, Get("key 1"));
			Assert.Equal(new[] { "value 2" }, Get("key 2"));
			Assert.Empty(Get("key 3"));

			IEnumerable<string> Get(string key)
			{
				foreach (var service in services)
					foreach (var value in service.Get(key))
						yield return value;
			}
		}

		[Fact]
		public void OnlyOneServiceIsRegistered()
		{
			var host = new AppHostBuilder()
				.UseServiceProviderFactory(new MicrosoftExtensionsServiceProviderFactory())
				.ConfigureServices<MultipleRegistrationBuilder>((context, builder) => builder.Add("key 1", "value 1"))
				.ConfigureServices<MultipleRegistrationBuilder>((context, builder) => builder.Add("key 2", "value 2"))
				.Build();

			var services = host.Services.GetServices<MappingService>().ToArray();

			var service = Assert.Single(services);

			Assert.Equal(new[] { "value 1" }, service.Get("key 1"));
			Assert.Equal(new[] { "value 2" }, service.Get("key 2"));
			Assert.Empty(service.Get("key 3"));
		}

		[Fact]
		public void EventsAreInvokedOnceAndInTheRightOrder()
		{
			var delegateInvoked = 0;
			var order = new List<string>();

			Assert.Equal(0, CountingServiceBuilder.Count);

			var host = new AppHostBuilder()
				.ConfigureServices<CountingServiceBuilder>((context, builder) =>
				{
					delegateInvoked++;
					order.Add("Delegate1");

					builder.BuildInvoked += services =>
					{
						order.Add("Build");
						Assert.NotNull(services);
					};

					builder.ConfigureInvoked += services =>
					{
						order.Add("Configure");
						Assert.NotNull(services);
					};
				})
				.ConfigureServices<CountingServiceBuilder>((context, builder) =>
				{
					delegateInvoked++;
					order.Add("Delegate2");
				})
				.ConfigureServices<CountingServiceBuilder>((context, builder) =>
				{
					delegateInvoked++;
					order.Add("Delegate3");
				})
				.Build();

			Assert.Equal(3, delegateInvoked);
			Assert.Equal(new[] { "Delegate1", "Delegate2", "Delegate3", "Build", "Configure" }, order);

			Assert.Equal(1, CountingServiceBuilder.Count);
		}

		[Fact]
		public void EventsAreInvokedWithTheCorrectParameters()
		{
			var host = new AppHostBuilder()
				.ConfigureServices<ServiceBuilderStub>((context, builder) =>
				{
					Assert.NotNull(context);
					Assert.NotNull(builder);
					builder.BuildInvoked += services =>
					{
						Assert.NotNull(services);
					};
					builder.ConfigureInvoked += services =>
					{
						Assert.NotNull(services);
					};
				})
				.Build();
		}

		class CountingServiceBuilder : ServiceBuilderStub
		{
			public static int Count { get; private set; } = 0;

			public CountingServiceBuilder()
			{
				Count++;
			}
		}

		class MultipleRegistrationBuilder : MappingService, IMauiServiceBuilder
		{
			public void ConfigureServices(IServiceCollection services)
			{
				services.AddSingleton<MappingService>(this);
			}

			public void Configure(IServiceProvider services)
			{
			}
		}

		class ServiceBuilderStub : IMauiServiceBuilder
		{
			public event Action<IServiceCollection> BuildInvoked;
			public event Action<IServiceProvider> ConfigureInvoked;

			public void ConfigureServices(IServiceCollection services)
			{
				BuildInvoked?.Invoke(services);
			}

			public void Configure(IServiceProvider services)
			{
				ConfigureInvoked?.Invoke(services);
			}
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