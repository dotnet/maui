using Microsoft.Maui.ApplicationModel;

namespace Maui.HybridApp;

/// <summary>A small host-thread boundary shared by providers, not an abstraction over Essentials APIs.</summary>
public sealed class NativeDispatcher
{
	readonly Func<Func<Task>, Task> _dispatch;

	/// <summary>Creates the boundary using the existing Essentials main-thread dispatcher.</summary>
	public NativeDispatcher() : this(action => MainThread.InvokeOnMainThreadAsync(action))
	{
	}

	/// <summary>Creates a boundary for a host with an explicit dispatch mechanism.</summary>
	/// <param name="dispatch">Runs and awaits the action on the required host thread.</param>
	public NativeDispatcher(Func<Func<Task>, Task> dispatch)
	{
		ArgumentNullException.ThrowIfNull(dispatch);
		_dispatch = dispatch;
	}

	/// <summary>Checks cancellation on the host thread before and after an OS operation.</summary>
	/// <typeparam name="T">The operation result type.</typeparam>
	/// <param name="action">The existing Essentials API operation; no browser fallback is used.</param>
	/// <param name="cancellation">The session's cooperative cancellation.</param>
	/// <returns>The native result; cancellation does not undo a completed OS write.</returns>
	public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellation)
	{
		T result = default!;
		await _dispatch(async () =>
		{
			cancellation.ThrowIfCancellationRequested();
			try
			{
				result = await action();
				cancellation.ThrowIfCancellationRequested();
			}
			catch (FeatureNotEnabledException)
			{
				throw new CommandException("configuration_required", "The native feature is disabled in system settings. Enable it before retrying.");
			}
			catch (PermissionException)
			{
				throw new CommandException("permission_denied", "The operating system denied permission for this native feature.");
			}
		}).ConfigureAwait(false);
		return result;
	}
	/// <summary>Posts scoped native event source setup/teardown without blocking a bridge invocation.</summary>
	/// <param name="action">The owning native source operation; invalidate stale callbacks before posting teardown.</param>
	/// <returns>Completion on the configured native thread.</returns>
	public Task PostAsync(Action action) => _dispatch(() => { action(); return Task.CompletedTask; });
}
