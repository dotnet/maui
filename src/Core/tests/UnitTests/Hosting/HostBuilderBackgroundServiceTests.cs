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
		public void HostedServicesRunBeforeTheAppStarts()
		{
			var builder = MauiApp.CreateBuilder();
			var startOrder = new List<object>();
			var app = new MockApp(startOrder);
			var hostedService = new HostedService(startOrder);

			builder.Services.AddSingleton<IHostedService>(hostedService);
			builder.Services.AddSingleton<IMauiInitializeService>(app);
			builder.Build();

			Assert.Equal(2, startOrder.Count);
			Assert.Same(hostedService, startOrder[0]);
			Assert.Same(app, startOrder[1]);
		}
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