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
		private readonly IServiceProvider _services;
		private readonly IServiceProvider _rootServices;

		internal MauiApp(IServiceProvider services, IServiceProvider rootServiceProvider)
		{
			_services = services;
			_rootServices = rootServiceProvider;
		}

		internal IServiceProvider RootServices => _rootServices;

		/// <summary>
		/// The application's configured services.
		/// </summary>
		public IServiceProvider Services => _services;

		/// <summary>
		/// The application's configured <see cref="IConfiguration"/>.
		/// </summary>
		public IConfiguration Configuration => _services.GetRequiredService<IConfiguration>();

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

			(_services as IDisposable)?.Dispose();
			(_rootServices as IDisposable)?.Dispose();
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			DisposeConfiguration();

			if (_services is IAsyncDisposable asyncDisposable)
			{
				// Fire and forget because this is called from a sync context
				await asyncDisposable.DisposeAsync();
			}
			else
			{
				(_services as IDisposable)?.Dispose();
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
