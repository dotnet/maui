using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using JavaObject = Java.Lang.Object;

namespace Microsoft.Maui.ApplicationModel;

/// <summary>
/// Represents a request for an activity result.
/// Provides a type-safe mechanism for registering and launching
/// activity result requests using the specified contract and callback.
/// </summary>
/// <typeparam name="TContract">The type of the activity result contract.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the activity.</typeparam>
/// <remarks>
/// <para>
/// <see href="https://developer.android.com/training/basics/intents/result">Google docs</see>
/// </para>
/// <para>
/// Each <see cref="ComponentActivity"/> instance gets its own registered launcher and its
/// own pending <see cref="TaskCompletionSource{TResult}"/> entry so that child activities
/// can use MediaPicker independently of the main activity, and so that two activities with
/// concurrent in-flight requests cannot clobber each other.
/// </para>
/// <para>
/// The result callback closes over the specific <see cref="ComponentActivity"/> instance
/// that was passed to <see cref="Register"/> and resolves the pending TCS using THAT
/// instance as the lookup key. Result delivery therefore does NOT depend on whichever
/// activity is "current" at delivery time — which is critical because the launching
/// activity may have been destroyed and recreated (rotation/config change) before the
/// picker returns.
/// </para>
/// </remarks>
internal abstract class ActivityForResultRequest<TContract, TResult>
	where TContract : ActivityResultContract, new()
	where TResult : JavaObject
{
	readonly Lock activeLaunchLock = new();

	// Tracks one ActivityResultLauncher per ComponentActivity instance.
	// ConditionalWeakTable holds weak references to keys, so entries are automatically
	// eligible for collection when the activity is no longer referenced.
	readonly ConditionalWeakTable<ComponentActivity, ActivityResultLauncher> _activityLaunchers = new();

	// Tracks pending TaskCompletionSource per ComponentActivity to prevent race conditions.
	// This prevents Activity B from overwriting Activity A's pending request.
	readonly ConditionalWeakTable<ComponentActivity, TaskCompletionSource<TResult>> _pendingRequests = new();

	/// <summary>
	/// Gets a value indicating whether the request has a launcher registered for the
	/// currently focused activity.
	/// </summary>
	protected bool HasLauncherForCurrentActivity => GetLauncherForCurrentActivity() is not null;

	/// <summary>
	/// Registers this request to start an activity for a result.
	/// Each <see cref="ComponentActivity"/> instance receives its own launcher so child
	/// activities can use MediaPicker independently of the main activity.
	/// </summary>
	/// <param name="componentActivity">The component activity to register the request with.</param>
	public void Register(ComponentActivity componentActivity)
	{
		if (componentActivity is null)
			throw new ArgumentNullException(nameof(componentActivity));

		// Skip if already registered for this specific activity instance (e.g. called again
		// after a no-op restart). Calling RegisterForActivityResult twice on the same
		// activity is not legal — must happen once during onCreate.
		if (_activityLaunchers.TryGetValue(componentActivity, out _))
			return;

		var contract = new TContract();

		// CRITICAL: capture the same `componentActivity` instance the launcher is being
		// registered for. The callback resolves the pending TCS for THIS specific activity,
		// NOT for whatever ActivityStateManager.Default.GetCurrentActivity() happens to be
		// at delivery time. That makes delivery invariant under rotation / config changes —
		// even if the activity is destroyed/recreated, the captured reference remains valid
		// for the duration of the in-flight callback (Android keeps the registered activity
		// alive long enough to deliver its own result).
		var registeredActivity = componentActivity;
		var callback = new ActivityResultCallback<TResult>(result => HandleActivityResult(registeredActivity, result));

		var launcher = componentActivity.RegisterForActivityResult(contract, callback);
		_activityLaunchers.Add(componentActivity, launcher);
	}

	/// <summary>
	/// Routes an AndroidX result that has no associated in-process activity to orphaned-result handling.
	/// </summary>
	/// <param name="result">The activity result.</param>
	/// <remarks>
	/// This entry point is used when AndroidX replays a pending result after process recreation.
	/// </remarks>
	protected void HandleActivityResult(TResult result)
		=> OnActivityResultForOrphanedLaunch(result);

	/// <summary>
	/// Handles a result delivered for an active launch request before the launch task is completed.
	/// </summary>
	/// <param name="result">The activity result.</param>
	protected virtual void OnActivityResultForActiveLaunch(TResult result)
	{
	}

	/// <summary>
	/// Handles a result delivered when there is no active launch request in this process.
	/// </summary>
	/// <param name="result">The activity result.</param>
	/// <remarks>
	/// AndroidX may deliver a pending result after the app process was recreated. In that case, the original
	/// launch task is gone and callers that persisted enough request state can reconcile the result here.
	/// </remarks>
	protected virtual void OnActivityResultForOrphanedLaunch(TResult result)
	{
	}

	void HandleActivityResult(ComponentActivity registeredActivity, TResult result)
	{
		var completionSource = TakePendingRequest(registeredActivity);
		if (completionSource is null)
		{
			OnActivityResultForOrphanedLaunch(result);
			return;
		}

		try
		{
			OnActivityResultForActiveLaunch(result);
			completionSource.TrySetResult(result);
		}
		catch (Exception ex)
		{
			completionSource.TrySetException(ex);
		}
	}

	/// <summary>
	/// Launches the activity result request with the specified input.
	/// </summary>
	/// <typeparam name="T">The type of the input parameter.</typeparam>
	/// <param name="input">The input parameter to launch the request with.</param>
	/// <returns>
	/// A task that represents the asynchronous operation, containing the result of the activity.
	/// </returns>
	public Task<TResult> Launch<T>(T input)
		where T : JavaObject
	{
		var launchingActivity = ActivityStateManager.Default.GetCurrentActivity() as ComponentActivity;
		if (launchingActivity is null)
		{
			Trace.WriteLine("""
			                ActivityForResultRequest.Launch() called but current activity is null or not a ComponentActivity.
			                Ensure your Activity inherits from ComponentActivity and call Microsoft.Maui.ApplicationModel.Platform.Init(Activity, Bundle) in OnCreate.
			                """);
			var canceledTcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			canceledTcs.SetCanceled();
			return canceledTcs.Task;
		}

		return Launch(launchingActivity, input);
	}

	/// <summary>
	/// Launches the activity result request for a specific activity instance.
	/// </summary>
	/// <typeparam name="T">The type of the input parameter.</typeparam>
	/// <param name="launchingActivity">The activity that owns the request lifecycle and launcher.</param>
	/// <param name="input">The input parameter to launch the request with.</param>
	/// <returns>
	/// A task that represents the asynchronous operation, containing the result of the activity.
	/// </returns>
	public Task<TResult> Launch<T>(ComponentActivity launchingActivity, T input)
		where T : JavaObject
	{
		if (launchingActivity is null)
			throw new ArgumentNullException(nameof(launchingActivity));

		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		lock (activeLaunchLock)
		{
			if (_pendingRequests.TryGetValue(launchingActivity, out _))
			{
				return Task.FromException<TResult>(new InvalidOperationException("An activity result request is already in progress."));
			}

			_pendingRequests.Add(launchingActivity, completionSource);
		}

		// Get the launcher for this specific activity
		if (!_activityLaunchers.TryGetValue(launchingActivity, out var launcher))
		{
			Trace.WriteLine("""
			                ActivityForResultRequest is not registered for the launching activity; cancelling the request.
			                Ensure your Activity inherits from ComponentActivity and call Microsoft.Maui.ApplicationModel.Platform.Init(Activity, Bundle) in OnCreate.
			                """);
			RemovePendingRequest(launchingActivity, completionSource);
			completionSource.TrySetCanceled();
			return completionSource.Task;
		}

		try
		{
			launcher.Launch(input);
		}
		catch (Exception ex)
		{
			RemovePendingRequest(launchingActivity, completionSource);
			completionSource.TrySetException(ex);
		}

		return completionSource.Task;
	}

	/// <summary>
	/// Cancels any pending request for the specified activity.
	/// This should be called from the activity's OnDestroy() or when the activity is being destroyed
	/// to ensure the pending task is completed rather than hanging indefinitely.
	/// </summary>
	/// <param name="componentActivity">The activity whose pending request should be cancelled.</param>
	internal void CancelPendingRequest(ComponentActivity componentActivity)
	{
		var completionSource = TakePendingRequest(componentActivity);
		completionSource?.TrySetCanceled();
	}

	TaskCompletionSource<TResult> TakePendingRequest(ComponentActivity componentActivity)
	{
		lock (activeLaunchLock)
		{
			if (!_pendingRequests.TryGetValue(componentActivity, out var completionSource))
				return null;

			_pendingRequests.Remove(componentActivity);
			return completionSource;
		}
	}

	void RemovePendingRequest(ComponentActivity componentActivity, TaskCompletionSource<TResult> completionSource)
	{
		lock (activeLaunchLock)
		{
			if (_pendingRequests.TryGetValue(componentActivity, out var activeCompletionSource) &&
				ReferenceEquals(activeCompletionSource, completionSource))
			{
				_pendingRequests.Remove(componentActivity);
			}
		}
	}

	ActivityResultLauncher GetLauncherForCurrentActivity()
	{
		if (ActivityStateManager.Default.GetCurrentActivity() is ComponentActivity currentActivity &&
			_activityLaunchers.TryGetValue(currentActivity, out var launcher))
		{
			return launcher;
		}

		return null;
	}
}
