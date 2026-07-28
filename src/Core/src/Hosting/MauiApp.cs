using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
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
		private int _disposeStarted;

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
		/// <remarks>
		/// When <see cref="Services"/> implements only <see cref="IAsyncDisposable"/>,
		/// this method blocks until asynchronous provider disposal completes. If provider
		/// disposal requires the calling thread to remain responsive, such as explicitly
		/// dispatching work to the UI thread, use <see cref="DisposeAsync"/> instead.
		/// Disposal runs at most once: only the first caller to <see cref="Dispose"/> or
		/// <see cref="DisposeAsync"/> performs teardown; a concurrent or later caller returns
		/// immediately without observing the in-flight disposal's completion or exceptions.
		/// </remarks>
		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
				return;

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
				if (_services is IDisposable disposable)
				{
					disposable.Dispose();
				}
				else if (_services is IAsyncDisposable asyncDisposable)
				{
					Task.Run(async () =>
					{
						await asyncDisposable.DisposeAsync().ConfigureAwait(false);
					}).GetAwaiter().GetResult();
				}
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}

			ThrowIfDisposalFailed(exceptions);
		}

		/// <inheritdoc />
		/// <remarks>
		/// Disposal runs at most once: only the first caller to <see cref="Dispose"/> or
		/// <see cref="DisposeAsync"/> performs teardown. A concurrent or later caller returns
		/// immediately with a completed <see cref="ValueTask"/> without awaiting the in-flight
		/// disposal and without observing any exception it throws. If you need to observe
		/// completion or exceptions, dispose from a single owner and await that call.
		/// </remarks>
		public async ValueTask DisposeAsync()
		{
			if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
				return;

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
