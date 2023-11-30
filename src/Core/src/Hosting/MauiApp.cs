using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A .NET MAUI application with registered services and configuration data.
	/// </summary>
	public sealed class MauiApp : IDisposable, IAsyncDisposable
	{
		private readonly IServiceProvider _applicationScopedServices;
		private readonly IServiceProvider _rootServices;

		internal MauiApp(IServiceProvider services, IServiceProvider rootServiceProvider)
		{
			_applicationScopedServices = services;
			_rootServices = rootServiceProvider;
		}

		internal IServiceProvider ScopedServices => _applicationScopedServices;

		/// <summary>
		/// The application's configured services.
		/// </summary>
		public IServiceProvider Services => _rootServices;

		/// <summary>
		/// The application's configured <see cref="IConfiguration"/>.
		/// </summary>
		public IConfiguration Configuration => _applicationScopedServices.GetRequiredService<IConfiguration>();

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiAppBuilder"/> class with optional defaults.
		/// </summary>
		/// <param name="useDefaults">Whether to create the <see cref="MauiAppBuilder"/> with common defaults.</param>
		/// <returns>The <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder CreateBuilder(bool useDefaults = true) => new(useDefaults);

		/// <inheritdoc />
		public void Dispose()
		{
			DisposeConfiguration();

			(_applicationScopedServices as IDisposable)?.Dispose();
			(_rootServices as IDisposable)?.Dispose();
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			DisposeConfiguration();
			await DisposeService(_applicationScopedServices);
			await DisposeService(_rootServices);

			async ValueTask DisposeService(IServiceProvider serviceProvider)
			{
				if (serviceProvider is IAsyncDisposable asyncDisposable)
				{
					// Fire and forget because this is called from a sync context
					await asyncDisposable.DisposeAsync();
				}
				else
				{
					(serviceProvider as IDisposable)?.Dispose();
				}
			}
		}

		private void DisposeConfiguration()
		{
			// Explicitly dispose the Configuration, since it is added as a singleton object that the ServiceProvider
			// won't dispose.
			(Configuration as IDisposable)?.Dispose();
		}
	}
}
