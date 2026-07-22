using System;
using System.Collections.Generic;
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
			var exceptions = new List<Exception>();
			try
			{
				CleanupAppServices();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			try
			{
				DisposeConfiguration();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			try
			{
				(_services as IDisposable)?.Dispose();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			ThrowIfDisposalFailed(exceptions);
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			var exceptions = new List<Exception>();
			try
			{
				CleanupAppServices();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			try
			{
				DisposeConfiguration();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			try
			{
				if (_services is IAsyncDisposable asyncDisposable)
				{
					await asyncDisposable.DisposeAsync().ConfigureAwait(false);
				}
				else
				{
					(_services as IDisposable)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			ThrowIfDisposalFailed(exceptions);
		}

		private void CleanupAppServices()
		{
			List<Exception>? exceptions = null;
			foreach (var cleanupService in _services.GetServices<IMauiAppCleanupService>())
			{
				try
				{
					cleanupService.Cleanup();
				}
				catch (Exception ex)
				{
					(exceptions ??= new()).Add(ex);
				}
			}

			if (exceptions is null)
				return;

			if (exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

			throw new AggregateException("One or more MauiApp cleanup services failed.", exceptions);
		}

		private void DisposeConfiguration()
		{
			// Explicitly dispose the Configuration, since it is added as a singleton object that the ServiceProvider
			// won't dispose.
			(Configuration as IDisposable)?.Dispose();
		}

		private static void ThrowIfDisposalFailed(List<Exception> exceptions)
		{
			if (exceptions.Count == 0)
				return;

			if (exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

			throw new AggregateException("MauiApp cleanup and disposal failed.", exceptions);
		}
	}

	internal interface IMauiAppCleanupService
	{
		void Cleanup();
	}
}
