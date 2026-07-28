using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

		// Coordinates the one-shot teardown. The first caller ("winner") runs the teardown and
		// completes _disposeCompletion; any other concurrent caller ("loser") awaits/blocks on the
		// same completion so it observes when teardown finishes and rethrows the winner's error.
		private readonly object _disposeGate = new();
		private readonly AsyncLocal<bool> _disposeScope = new();
		private TaskCompletionSource<ExceptionDispatchInfo?>? _disposeCompletion;

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
		/// <para>
		/// Disposal runs exactly once. When several callers race <see cref="Dispose"/> and/or
		/// <see cref="DisposeAsync"/> on the same instance, the first caller performs the teardown
		/// and every other caller waits for it to finish and observes the same exception. A caller
		/// that re-enters disposal from the logical execution flow already performing it returns
		/// immediately without waiting to avoid deadlocking on itself, and therefore does not observe
		/// completion or exceptions.
		/// </para>
		/// </remarks>
		public void Dispose()
		{
			if (!TryBeginDispose(out var completion))
			{
				WaitForDisposal(completion);
				return;
			}

			var exceptions = new List<Exception>();
			try
			{
				RunSharedCleanup(exceptions);

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
			}
			finally
			{
				FinishDisposal(completion, exceptions);
			}
		}

		/// <inheritdoc />
		/// <remarks>
		/// Disposal runs exactly once. When several callers race <see cref="Dispose"/> and/or
		/// <see cref="DisposeAsync"/> on the same instance, the first caller performs the teardown
		/// and every other caller awaits its completion and observes the same exception. A caller
		/// that re-enters disposal from the logical execution flow already performing it returns
		/// immediately without awaiting to avoid deadlocking on itself, and therefore does not
		/// observe completion or exceptions.
		/// </remarks>
		public async ValueTask DisposeAsync()
		{
			if (!TryBeginDispose(out var completion))
			{
				await WaitForDisposalAsync(completion).ConfigureAwait(false);
				return;
			}

			var exceptions = new List<Exception>();
			try
			{
				RunSharedCleanup(exceptions);

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
			}
			finally
			{
				FinishDisposal(completion, exceptions);
			}
		}

		// Returns true and hands back a fresh completion when the caller wins the one-shot race and
		// must perform the teardown. Returns false otherwise: 'completion' is the shared completion to
		// wait on, or null when the current logical flow is re-entering teardown or teardown is complete.
		private bool TryBeginDispose([NotNullWhen(true)] out TaskCompletionSource<ExceptionDispatchInfo?>? completion)
		{
			lock (_disposeGate)
			{
				if (_disposeScope.Value)
				{
					completion = null;
					return false;
				}

				if (_disposeCompletion is not null)
				{
					completion = _disposeCompletion.Task.IsCompleted ? null : _disposeCompletion;
					return false;
				}

				completion = _disposeCompletion = new TaskCompletionSource<ExceptionDispatchInfo?>(
					TaskCreationOptions.RunContinuationsAsynchronously);
				_disposeScope.Value = true;
				return true;
			}
		}

		private void RunSharedCleanup(List<Exception> exceptions)
		{
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
		}

		private void FinishDisposal(TaskCompletionSource<ExceptionDispatchInfo?> completion, List<Exception> exceptions)
		{
			try
			{
				CompleteDisposal(completion, exceptions);
			}
			finally
			{
				_disposeScope.Value = false;
			}
		}

		// Publishes the teardown outcome to every concurrent caller before the winner rethrows it.
		private static void CompleteDisposal(TaskCompletionSource<ExceptionDispatchInfo?> completion, List<Exception> exceptions)
		{
			ExceptionDispatchInfo? error = null;
			if (exceptions.Count == 1)
				error = ExceptionDispatchInfo.Capture(exceptions[0]);
			else if (exceptions.Count > 1)
				error = ExceptionDispatchInfo.Capture(
					new AggregateException("MauiApp cleanup and disposal failed.", exceptions));

			completion.SetResult(error);
			error?.Throw();
		}

		private static void WaitForDisposal(TaskCompletionSource<ExceptionDispatchInfo?>? completion)
		{
			if (completion is null)
				return;

			completion.Task.GetAwaiter().GetResult()?.Throw();
		}

		private static async ValueTask WaitForDisposalAsync(TaskCompletionSource<ExceptionDispatchInfo?>? completion)
		{
			if (completion is null)
				return;

			(await completion.Task.ConfigureAwait(false))?.Throw();
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
	}

	internal interface IMauiAppCleanupService
	{
		void Cleanup();
	}
}
