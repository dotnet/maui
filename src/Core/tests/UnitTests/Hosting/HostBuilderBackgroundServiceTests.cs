using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderBackgroundServiceTests
	{
		[Fact]
		public async Task HostedServicesRunBeforeTheAppStarts()
		{
			var builder = MauiApp.CreateBuilder();
			var startOrder = new List<object>();
			var app = new MockApp(startOrder);
			var hostedService = new HostedService(startOrder);

			builder.Services.AddSingleton<IHostedService>(hostedService);
			builder.Services.AddSingleton<IMauiInitializeService>(app);
			var mauiApp = builder.Build();

			var cts = new CancellationTokenSource();
			await mauiApp.StartAsync(cts.Token);
			Assert.Equal(2, startOrder.Count);
			Assert.Same(app, startOrder[0]);
			Assert.Same(hostedService, startOrder[1]);
		}

		[Fact]
		public async Task HostedServicesMultipleBlockingServices()
		{
			var builder = MauiApp.CreateBuilder();
			var startOrder = new List<object>();

			builder.Services.AddSingleton(startOrder);
			builder.Services.AddHostedService<BlockingService1>();
			builder.Services.AddHostedService<BlockingService2>();
			var app = builder.Build();

			var cts = new CancellationTokenSource();
			await app.StartAsync(cts.Token);

			Assert.Equal(2, startOrder.Count);

			await app.StopAsync(cts.Token);
		}
	}

	class BlockingService2 : IHostedService
	{
		private readonly List<object> _startOrder;

		public BlockingService2(List<object> startOrder)
		{
			_startOrder = startOrder;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_startOrder.Add(this);
			return Task.Delay(1000, cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}

	class BlockingService1 : IHostedService
	{
		private readonly List<object> _startOrder;

		public BlockingService1(List<object> startOrder)
		{
			_startOrder = startOrder;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{

			_startOrder.Add(this);
			return Task.Delay(2000, cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}

	class HostedService : IHostedService
	{
		private readonly List<object> _startOrder;

		public HostedService(List<object> startOrder)
		{
			_startOrder = startOrder;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_startOrder.Add(this);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}

	class MockApp : IMauiInitializeService
	{
		private readonly List<object> _startOrder;

		public MockApp(List<object> startOrder)
		{
			_startOrder = startOrder;
		}

		public void Dispose() { }

		public void Initialize(IServiceProvider services)
		{
			_startOrder.Add(this);
		}
	}
}