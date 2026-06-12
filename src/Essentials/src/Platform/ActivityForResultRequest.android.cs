using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
/// This must be unconditionally registered every time our activity is created.
/// Each <see cref="ComponentActivity"/> instance gets its own launcher so that
/// multi-activity apps can call MediaPicker independently from any activity in
/// the back stack without invalidating launchers registered by other activities.
/// </remarks>
internal abstract class ActivityForResultRequest<TContract, TResult>
	where TContract : ActivityResultContract, new()
	where TResult : JavaObject
{
	// Tracks one ActivityResultLauncher per ComponentActivity instance.
	// ConditionalWeakTable holds weak references to keys, so entries are automatically
	// eligible for collection when the activity is no longer referenced.
	readonly ConditionalWeakTable<ComponentActivity, ActivityResultLauncher> _activityLaunchers = new();

	// Tracks pending TaskCompletionSource per ComponentActivity to prevent race conditions.
	// This prevents Activity B from overwriting Activity A's pending request.
	readonly ConditionalWeakTable<ComponentActivity, TaskCompletionSource<TResult>> _pendingRequests = new();

	/// <summary>
	/// Gets a value indicating whether the request has a launcher registered for the current activity.
	/// </summary>
	/// <remarks>
	/// This property name was clarified from <c>IsRegistered</c> to better reflect that it checks
	/// for a launcher for the currently-focused activity, not whether any registration has occurred.
	/// </remarks>
	protected bool HasLauncherForCurrentActivity => GetLauncherForCurrentActivity() is not null;

	// Deprecated: Use HasLauncherForCurrentActivity instead
	[Obsolete("Use HasLauncherForCurrentActivity instead. IsRegistered is misleading because it only checks the current activity.", false)]
	protected bool IsRegistered => HasLauncherForCurrentActivity;

	/// <summary>
	/// Registers this request to start an activity for a result.
	/// Each <see cref="ComponentActivity"/> instance receives its own launcher so that
	/// child activities can use MediaPicker independently of the main activity.
	/// </summary>
	/// <param name="componentActivity">The component activity to register the request with.</param>
	public void Register(ComponentActivity componentActivity)
	{
		// Skip if already registered for this specific activity instance (e.g. called again after config change).
		if (_activityLaunchers.TryGetValue(componentActivity, out _))
			return;

		var contract = new TContract();

		var activity = componentActivity;
		var callback = new ActivityResultCallback<TResult>(result =>
		{
			// Only complete if this activity has a pending request
			if (_pendingRequests.TryGetValue(activity, out var tcs))
			{
				_pendingRequests.Remove(activity);
				tcs?.SetResult(result);
			}
		});

		var launcher = componentActivity.RegisterForActivityResult(contract, callback);
		_activityLaunchers.Add(componentActivity, launcher);
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
			                ActivityForResultRequest.Launch() called but current activity is null.
			                Ensure your Activity inherits from ComponentActivity and call Microsoft.Maui.ApplicationModel.Platform.Init(Activity, Bundle) in OnCreate.
			                """);
			var canceledTcs = new TaskCompletionSource<TResult>();
			canceledTcs.SetCanceled();
			return canceledTcs.Task;
		}

		if (_pendingRequests.TryGetValue(launchingActivity, out var existingTcs))
		{
			_pendingRequests.Remove(launchingActivity);
			existingTcs?.TrySetCanceled();
		}

		var tcs = new TaskCompletionSource<TResult>();
		_pendingRequests.Add(launchingActivity, tcs);

		// Get the launcher for this specific activity
		if (!_activityLaunchers.TryGetValue(launchingActivity, out var launcher))
		{
			Trace.WriteLine("""
			                ActivityForResultRequest is not registered for the launching activity; cancelling the request.
			                Ensure your Activity inherits from ComponentActivity and call Microsoft.Maui.ApplicationModel.Platform.Init(Activity, Bundle) in OnCreate.
			                """);
			_pendingRequests.Remove(launchingActivity);
			tcs.SetCanceled();
			return tcs.Task;
		}

		try
		{
			launcher.Launch(input);
		}
		catch (Exception ex)
		{
			_pendingRequests.Remove(launchingActivity);
			tcs.SetException(ex);
		}

		return tcs.Task;
	}

	/// <summary>
	/// Cancels any pending request for the specified activity.
	/// This should be called from the activity's OnDestroy() or when the activity is being destroyed
	/// to ensure the pending task is completed rather than hanging indefinitely.
	/// </summary>
	/// <param name="componentActivity">The activity whose pending request should be cancelled.</param>
	internal void CancelPendingRequest(ComponentActivity componentActivity)
	{
		if (_pendingRequests.TryGetValue(componentActivity, out var tcs))
		{
			_pendingRequests.Remove(componentActivity);
			tcs?.TrySetCanceled();
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
