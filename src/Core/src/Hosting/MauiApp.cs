using System;
using System.Runtime.ExceptionServices;
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
			DisposeConfiguration();

			Exception? cleanupException = null;
			try
			{
				CleanupAppServices();
			}
			catch (Exception ex)
			{
				cleanupException = ex;
			}

			try
			{
				(_services as IDisposable)?.Dispose();
			}
			catch (Exception disposalException)
			{
				if (cleanupException is not null)
				{
					throw new AggregateException(
						"MauiApp cleanup and service provider disposal both failed.",
						cleanupException,
						disposalException);
				}

				throw;
			}

			if (cleanupException is not null)
				ExceptionDispatchInfo.Capture(cleanupException).Throw();
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			DisposeConfiguration();

			Exception? cleanupException = null;
			try
			{
				CleanupAppServices();
			}
			catch (Exception ex)
			{
				cleanupException = ex;
			}

			try
			{
				if (_services is IAsyncDisposable asyncDisposable)
				{
					await asyncDisposable.DisposeAsync();
				}
				else
				{
					(_services as IDisposable)?.Dispose();
				}
			}
			catch (Exception disposalException)
			{
				if (cleanupException is not null)
				{
					throw new AggregateException(
						"MauiApp cleanup and service provider disposal both failed.",
						cleanupException,
						disposalException);
				}

				throw;
			}

			if (cleanupException is not null)
				ExceptionDispatchInfo.Capture(cleanupException).Throw();
		}

		private void CleanupAppServices()
		{
			foreach (var cleanupService in _services.GetServices<IMauiAppCleanupService>())
				cleanupService.Cleanup();
		}

		private void DisposeConfiguration()
		{
			// Explicitly dispose the Configuration, since it is added as a singleton object that the ServiceProvider
			// won't dispose.
			(Configuration as IDisposable)?.Dispose();
		}
	}

	internal interface IMauiAppCleanupService
	{
		void Cleanup();
	}
}
