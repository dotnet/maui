using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Hosting.Internal
{
	internal class AppHost : IHost, IAsyncDisposable
	{
		readonly ILogger<AppHost>? _logger;

		public AppHost(IServiceProvider services, ILogger<AppHost>? logger)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
			_logger = logger;
		}

		public IServiceProvider Services { get; }

		public async Task StartAsync(CancellationToken cancellationToken = default)
		{
			_logger?.Starting();

			await Task.Run(() => { });

			_logger?.Started();
		}

		public async Task StopAsync(CancellationToken cancellationToken = default)
		{
			_logger?.Stopping();

			_ = await Task.FromResult(new object());

			_logger?.Stopped();
		}

		public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

		public async ValueTask DisposeAsync()
		{
			switch (Services)
			{
				case IAsyncDisposable asyncDisposable:
					await asyncDisposable.DisposeAsync().ConfigureAwait(false);
					break;
				case IDisposable disposable:
					disposable.Dispose();
					break;
			}
		}
	}
}
