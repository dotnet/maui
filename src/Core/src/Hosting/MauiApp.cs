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
		private readonly MauiAppInitializationState _initializationState;

		// Coordinates the one-shot teardown. The first caller ("winner") runs the teardown and
		// completes _disposeCompletion; any other concurrent caller ("loser") awaits/blocks on the
		// same completion so it observes when teardown finishes and rethrows the winner's error.
		private readonly object _disposeGate = new();
		private readonly AsyncLocal<DisposalScope?> _disposeScope = new();
		private readonly AsyncLocal<InitializationScope?> _initializeScope = new();
		private TaskCompletionSource<ExceptionDispatchInfo?>? _disposeCompletion;
		private TaskCompletionSource<bool>? _initializeCompletion;
		private bool _hasCompletedInitialization;
		private int _initializeInFlight;

		internal MauiApp(IServiceProvider services)
		{
			_services = services;
			_initializationState = services.GetRequiredService<MauiAppInitializationState>();
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
		/// When <see cref="Services"/> implements <see cref="IAsyncDisposable"/>,
		/// this method blocks until asynchronous provider disposal completes. If provider
		/// disposal requires the calling thread to remain responsive, such as explicitly
		/// dispatching work to the UI thread, use <see cref="DisposeAsync"/> instead.
		/// <para>
		/// Disposal runs exactly once. When several callers race <see cref="Dispose"/> and/or
		/// <see cref="DisposeAsync"/> on the same instance, the first caller performs the teardown
		/// and every other caller waits for it to finish and observes the same exception. A caller
		/// that re-enters disposal from the logical execution flow already performing it returns
		/// immediately without waiting to avoid deadlocking on itself, and therefore does not observe
		/// completion or exceptions. After teardown completes, subsequent non-reentrant calls return
		/// immediately on success or rethrow the recorded teardown exception.
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
				WaitForInitializeAppServices(exceptions);
				var postProviderCleanupServices = CapturePostProviderCleanupServices(exceptions);
				RunSharedCleanup(exceptions);

				try
				{
					if (_services is IAsyncDisposable asyncDisposable)
					{
						Task.Run(async () =>
						{
							await asyncDisposable.DisposeAsync().ConfigureAwait(false);
						}).GetAwaiter().GetResult();
					}
					else if (_services is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}

				CleanupPostProviderServices(postProviderCleanupServices, exceptions);
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
		/// observe completion or exceptions. After teardown completes, subsequent non-reentrant calls
		/// return immediately on success or rethrow the recorded teardown exception.
		/// <para>
		/// When app-service initialization is already in flight, this method returns an incomplete
		/// operation and waits asynchronously for initialization to finish before teardown begins.
		/// </para>
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
				await WaitForInitializeAppServicesAsync(exceptions).ConfigureAwait(false);
				var postProviderCleanupServices = CapturePostProviderCleanupServices(exceptions);
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

				CleanupPostProviderServices(postProviderCleanupServices, exceptions);
			}
			finally
			{
				FinishDisposal(completion, exceptions);
			}
		}

		// Returns true and hands back a fresh completion when the caller wins the one-shot race and
		// must perform the teardown. Returns false otherwise: 'completion' is the shared completion to
		// wait on, or null when the current logical flow is re-entering teardown.
		private bool TryBeginDispose(
			[NotNullWhen(true)] out TaskCompletionSource<ExceptionDispatchInfo?>? completion)
		{
			lock (_disposeGate)
			{
				if (_disposeScope.Value is { IsActive: true })
				{
					completion = null;
					return false;
				}

				if (IsInitializeAppServicesActive())
					throw new InvalidOperationException("MauiApp cannot be disposed from app-service initialization.");

				if (_disposeCompletion is not null)
				{
					completion = _disposeCompletion;
					return false;
				}

				completion = _disposeCompletion = new TaskCompletionSource<ExceptionDispatchInfo?>(
					TaskCreationOptions.RunContinuationsAsynchronously);
				_disposeScope.Value = new DisposalScope();

				return true;
			}
		}

		internal void EnterInitializeAppServices()
		{
			lock (_disposeGate)
			{
				if (_disposeCompletion is not null)
					throw new ObjectDisposedException(nameof(MauiApp), "Cannot initialize app services after MauiApp disposal has begun.");

				_initializationState.Enter(isInitialBuild: !_hasCompletedInitialization);
				if (_initializeInFlight == 0)
				{
					_initializeCompletion = new TaskCompletionSource<bool>(
						TaskCreationOptions.RunContinuationsAsynchronously);
				}

				_initializeInFlight++;
				// ExecutionContext copies AsyncLocal references into child tasks. Use one frame per
				// entry so a child flow cannot mutate the parent's active-scope state.
				_initializeScope.Value = new InitializationScope(_initializeScope.Value);
			}
		}

		internal void ExitInitializeAppServices(bool initializationSucceeded = true)
		{
			lock (_disposeGate)
			{
				if (initializationSucceeded)
					_hasCompletedInitialization = true;

				if (_initializeScope.Value is { } scope)
				{
					scope.IsActive = false;
					_initializeScope.Value = scope.Parent;
				}

				_initializationState.Exit();
				if (--_initializeInFlight == 0)
				{
					Monitor.PulseAll(_disposeGate);
					_initializeCompletion?.TrySetResult(true);
					_initializeCompletion = null;
				}
			}
		}

		private bool IsInitializeAppServicesActive()
		{
			for (var scope = _initializeScope.Value; scope is not null; scope = scope.Parent)
			{
				if (scope.IsActive)
					return true;
			}

			return false;
		}

		private async ValueTask WaitForInitializeAppServicesAsync(List<Exception> exceptions)
		{
			while (true)
			{
				Task completion;
				lock (_disposeGate)
				{
					if (_initializeInFlight == 0)
						return;

					if (_initializeCompletion is null || _initializeCompletion.Task.IsCompleted)
					{
						_initializeCompletion = new TaskCompletionSource<bool>(
							TaskCreationOptions.RunContinuationsAsynchronously);
					}

					completion = _initializeCompletion.Task;
				}

				try
				{
					await completion.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
		}

		private void WaitForInitializeAppServices(List<Exception> exceptions)
		{
			while (true)
			{
				try
				{
					lock (_disposeGate)
					{
						while (_initializeInFlight > 0)
							Monitor.Wait(_disposeGate);
					}

					return;
				}
				catch (ThreadInterruptedException ex)
				{
					exceptions.Add(ex);
				}
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

		private List<IMauiAppPostProviderCleanupService> CapturePostProviderCleanupServices(List<Exception> exceptions)
		{
			try
			{
				return new List<IMauiAppPostProviderCleanupService>(
					_services.GetServices<IMauiAppPostProviderCleanupService>());
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
				return new();
			}
		}

		private static void CleanupPostProviderServices(
			List<IMauiAppPostProviderCleanupService> cleanupServices,
			List<Exception> exceptions)
		{
			foreach (var cleanupService in cleanupServices)
			{
				try
				{
					cleanupService.Cleanup();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
		}

		private void FinishDisposal(TaskCompletionSource<ExceptionDispatchInfo?> completion, List<Exception> exceptions)
		{
			if (_disposeScope.Value is { } scope)
			{
				scope.IsActive = false;
				_disposeScope.Value = null;
			}

			CompleteDisposal(completion, exceptions);
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

		private sealed class InitializationScope
		{
			public InitializationScope(InitializationScope? parent)
			{
				Parent = parent;
			}

			public InitializationScope? Parent { get; }

			public bool IsActive { get; set; } = true;
		}

		private sealed class DisposalScope
		{
			public bool IsActive { get; set; } = true;
		}
	}

	internal interface IMauiAppCleanupService
	{
		void Cleanup();
	}

	internal interface IMauiAppPostProviderCleanupService
	{
		void Cleanup();
	}

	internal sealed class MauiAppInitializationState
	{
		private readonly AsyncLocal<InitializationStateScope?> _scope = new();

		public bool IsInitialBuild =>
			_scope.Value is { IsActive: true, IsInitialBuild: true };

		public void Enter(bool isInitialBuild)
		{
			_scope.Value = new InitializationStateScope(isInitialBuild, _scope.Value);
		}

		public void Exit()
		{
			if (_scope.Value is not { } scope)
				return;

			scope.IsActive = false;
			_scope.Value = scope.Parent;
		}

		private sealed class InitializationStateScope
		{
			public InitializationStateScope(bool isInitialBuild, InitializationStateScope? parent)
			{
				IsInitialBuild = isInitialBuild;
				Parent = parent;
			}

			public bool IsInitialBuild { get; }

			public InitializationStateScope? Parent { get; }

			public bool IsActive { get; set; } = true;
		}
	}
}
