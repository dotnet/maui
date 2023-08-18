using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A .NET MAUI application with registered services and configuration data.
	/// </summary>
	public sealed class MauiApp : IDisposable
	{
		private readonly IServiceProvider _services;

		internal MauiApp(IServiceProvider services)
		{
			_services = services;
		}

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
			// Explicitly dispose the Configuration, since it is added as a singleton object that the ServiceProvider
			// won't dispose.
			(Configuration as IDisposable)?.Dispose();

			(_services as IDisposable)?.Dispose();
		}
	}
}
