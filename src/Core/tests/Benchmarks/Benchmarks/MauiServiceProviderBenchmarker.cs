using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class MauiServiceProviderBenchmarker
	{
		IAppHost _host;

		[Params(100_000)]
		public int N { get; set; }

		[IterationSetup(Target = nameof(DefaultBuilder))]
		public void SetupForDefaultBuilder()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.ConfigureServices((ctx, svc) => svc.AddTransient<IFooService, FooService>())
				.Build();
		}

		[IterationSetup(Target = nameof(DefaultBuilderWithConstructorInjection))]
		public void SetupForDefaultBuilderWithConstructorInjection()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, svc) => svc.AddTransient<IFooService, FooService>())
				.Build();
		}

		[IterationSetup(Target = nameof(OneConstructorParameter))]
		public void SetupForOneConstructorParameter()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, svc) =>
				{
					svc.AddTransient<IFooService, FooService>();
					svc.AddTransient<IFooBarService, FooBarWithFooService>();
				})
				.Build();
		}

		[IterationSetup(Target = nameof(TwoConstructorParameters))]
		public void SetupForTwoConstructorParameters()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, svc) =>
				{
					svc.AddTransient<IFooService, FooService>();
					svc.AddTransient<IBarService, BarService>();
					svc.AddTransient<IFooBarService, FooBarWithFooAndBarService>();
				})
				.Build();
		}

		[IterationSetup(Target = nameof(ExtensionsWithConstructorInjection))]
		public void SetupForExtensionsWithConstructorInjection()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory())
				.ConfigureServices((ctx, svc) => svc.AddTransient<IFooService, FooService>())
				.Build();
		}

		[IterationSetup(Target = nameof(ExtensionsWithOneConstructorParameter))]
		public void SetupForExtensionsWithOneConstructorParameter()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory())
				.ConfigureServices((ctx, svc) =>
				{
					svc.AddTransient<IFooService, FooService>();
					svc.AddTransient<IFooBarService, FooBarWithFooService>();
				})
				.Build();
		}

		[IterationSetup(Target = nameof(ExtensionsWithTwoConstructorParameters))]
		public void SetupForExtensionsWithTwoConstructorParameters()
		{
			_host = AppHost
				.CreateDefaultBuilder()
				.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory())
				.ConfigureServices((ctx, svc) =>
				{
					svc.AddTransient<IFooService, FooService>();
					svc.AddTransient<IBarService, BarService>();
					svc.AddTransient<IFooBarService, FooBarWithFooAndBarService>();
				})
				.Build();
		}

		[Benchmark(Baseline = true)]
		public void DefaultBuilder()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooService>();
			}
		}

		[Benchmark]
		public void DefaultBuilderWithConstructorInjection()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooService>();
			}
		}

		[Benchmark]
		public void OneConstructorParameter()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooBarService>();
			}
		}

		[Benchmark]
		public void TwoConstructorParameters()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooBarService>();
			}
		}

		[Benchmark]
		public void ExtensionsWithConstructorInjection()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooService>();
			}
		}

		[Benchmark]
		public void ExtensionsWithOneConstructorParameter()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooBarService>();
			}
		}

		[Benchmark]
		public void ExtensionsWithTwoConstructorParameters()
		{
			for (int i = 0; i < N; i++)
			{
				_host.Services.GetService<IFooBarService>();
			}
		}

		public class DIExtensionsServiceProviderFactory : IServiceProviderFactory<ServiceCollection>
		{
			public ServiceCollection CreateBuilder(IServiceCollection services)
				=> new ServiceCollection { services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
				=> containerBuilder.BuildServiceProvider();
		}
	}
}