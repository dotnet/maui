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
		IServiceProvider _serviceProvider;

		[Params(100_000)]
		public int N { get; set; }

		[IterationSetup(Target = nameof(DefaultBuilder))]
		public void SetupForDefaultBuilder()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[IterationSetup(Target = nameof(DefaultBuilderWithConstructorInjection))]
		public void SetupForDefaultBuilderWithConstructorInjection()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[IterationSetup(Target = nameof(OneConstructorParameter))]
		public void SetupForOneConstructorParameter()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IFooBarService, FooBarWithFooService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[IterationSetup(Target = nameof(TwoConstructorParameters))]
		public void SetupForTwoConstructorParameters()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooBarWithFooAndBarService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[IterationSetup(Target = nameof(ExtensionsWithConstructorInjection))]
		public void SetupForExtensionsWithConstructorInjection()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[IterationSetup(Target = nameof(ExtensionsWithOneConstructorParameter))]
		public void SetupForExtensionsWithOneConstructorParameter()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IFooBarService, FooBarWithFooService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[IterationSetup(Target = nameof(ExtensionsWithTwoConstructorParameters))]
		public void SetupForExtensionsWithTwoConstructorParameters()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooBarWithFooAndBarService>();

			var mauiApp = builder.Build();
			_serviceProvider = mauiApp.Services;
		}

		[Benchmark(Baseline = true)]
		public void DefaultBuilder()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooService>();
			}
		}

		[Benchmark]
		public void DefaultBuilderWithConstructorInjection()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooService>();
			}
		}

		[Benchmark]
		public void OneConstructorParameter()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooBarService>();
			}
		}

		[Benchmark]
		public void TwoConstructorParameters()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooBarService>();
			}
		}

		[Benchmark]
		public void ExtensionsWithConstructorInjection()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooService>();
			}
		}

		[Benchmark]
		public void ExtensionsWithOneConstructorParameter()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooBarService>();
			}
		}

		[Benchmark]
		public void ExtensionsWithTwoConstructorParameters()
		{
			for (int i = 0; i < N; i++)
			{
				_serviceProvider.GetService<IFooBarService>();
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