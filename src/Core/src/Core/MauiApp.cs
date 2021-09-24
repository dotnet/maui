using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public sealed class MauiApp : IHost
	{
		private readonly IHost _host;

		internal MauiApp(IHost host)
		{
			_host = host;
			Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(Environment.ApplicationName);
		}

		/// <summary>
		/// The application's configured services.
		/// </summary>
		public IServiceProvider Services => _host.Services;

		/// <summary>
		/// The application's configured <see cref="IConfiguration"/>.
		/// </summary>
		public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

		/// <summary>
		/// The application's configured <see cref="IHostEnvironment"/>.
		/// </summary>
		public IHostEnvironment Environment => _host.Services.GetRequiredService<IHostEnvironment>();

		/// <summary>
		/// Allows consumers to be notified of application lifetime events.
		/// </summary>
		public IHostApplicationLifetime Lifetime => _host.Services.GetRequiredService<IHostApplicationLifetime>();

		/// <summary>
		/// The default logger for the application.
		/// </summary>
		public ILogger Logger { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiApp"/> class with preconfigured defaults.
		/// </summary>
		/// <returns>The <see cref="MauiApp"/>.</returns>
		public static MauiApp Create() => new MauiAppBuilder().Build();

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiAppBuilder"/> class with preconfigured defaults.
		/// </summary>
		/// <returns>The <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder CreateBuilder() => new();

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiAppBuilder"/> class with optional defaults.
		/// </summary>
		/// <param name="useDefaults">Whether to create the <see cref="MauiAppBuilder"/> with common defaults.</param>
		/// <returns>The <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder CreateBuilder(bool useDefaults = true) => new(useDefaults);

		/// <summary>
		/// Start the application.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// A <see cref="Task"/> that represents the startup of the <see cref="MauiApp"/>.
		/// Successful completion indicates the HTTP server is ready to accept new requests.
		/// </returns>
		public Task StartAsync(CancellationToken cancellationToken = default) =>
			_host.StartAsync(cancellationToken);

		/// <summary>
		/// Shuts down the application.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// A <see cref="Task"/> that represents the shutdown of the <see cref="MauiApp"/>.
		/// Successful completion indicates that all the HTTP server has stopped.
		/// </returns>
		public Task StopAsync(CancellationToken cancellationToken = default) =>
			_host.StopAsync(cancellationToken);

		/// <summary>
		/// Disposes the application.
		/// </summary>
		void IDisposable.Dispose() => _host.Dispose();
	}
}
