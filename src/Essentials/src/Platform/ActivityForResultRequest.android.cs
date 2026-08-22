#nullable enable annotations
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Bundle = Android.OS.Bundle;
using JavaObject = Java.Lang.Object;

namespace Microsoft.Maui.ApplicationModel;

internal interface IActivityForResultRequest
{
	void Register(ComponentActivity componentActivity, Bundle? savedInstanceState);
	void SaveInstanceState(ComponentActivity componentActivity, Bundle outState);
	void ActivityDestroyed(ComponentActivity componentActivity);
}

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
/// Pending requests use an owner identifier saved in the activity instance state. A callback
/// registered by a recreated activity can therefore resolve the task started by the previous
/// activity instance, including recreation that does not retain the ViewModelStore.
/// </para>
/// </remarks>
internal abstract class ActivityForResultRequest<TContract, TResult>
	: IActivityForResultRequest
	where TContract : ActivityResultContract, new()
	where TResult : JavaObject
{
	static readonly TimeSpan TaskPresencePollInterval = TimeSpan.FromSeconds(2);

	static readonly string RequestOwnerStateKey =
		$"Microsoft.Maui.ApplicationModel.ActivityForResultRequest.{typeof(TContract).FullName}";

	// Tracks one ActivityResultLauncher per ComponentActivity instance.
	// ConditionalWeakTable holds weak references to keys, so entries are automatically
	// eligible for collection when the activity is no longer referenced.
	readonly ConditionalWeakTable<ComponentActivity, ActivityResultLauncher> _activityLaunchers = new();

	readonly ConditionalWeakTable<ComponentActivity, string> _requestOwners = new();
	readonly ConditionalWeakTable<ComponentActivity, object> _savedRequestOwners = new();
	readonly ConcurrentDictionary<string, System.Threading.CancellationTokenSource> _taskRemovalMonitors = new();
	readonly ActivityForResultRequestState<TResult> _requestState = new(RequestOwnerStateKey);

	/// <summary>
	/// Registers this request to start an activity for a result.
	/// Each <see cref="ComponentActivity"/> instance receives its own launcher so child
	/// activities can use MediaPicker independently of the main activity.
	/// </summary>
	/// <param name="componentActivity">The component activity to register the request with.</param>
	/// <param name="savedInstanceState">State restored for a recreated activity, if available.</param>
	public void Register(ComponentActivity componentActivity, Bundle? savedInstanceState)
	{
		if (componentActivity is null)
			throw new ArgumentNullException(nameof(componentActivity));

		// Skip if already registered for this specific activity instance (e.g. called again
		// after a no-op restart). Calling RegisterForActivityResult twice on the same
		// activity is not legal — must happen once during onCreate.
		if (_activityLaunchers.TryGetValue(componentActivity, out _))
			return;

		var contract = new TContract();

		var requestOwner = _requestState.RestoreOrCreateOwner(savedInstanceState);
		StopTaskRemovalMonitor(requestOwner);
		var callback = new ActivityResultCallback<TResult>(result =>
		{
			if (_requestState.TrySetResult(requestOwner, result))
				StopTaskRemovalMonitor(requestOwner);
		});

		var launcher = RegisterForActivityResult(componentActivity, contract, callback);
		_requestOwners.Add(componentActivity, requestOwner);
		_activityLaunchers.Add(componentActivity, launcher);
	}

	protected virtual ActivityResultLauncher RegisterForActivityResult(
		ComponentActivity componentActivity,
		TContract contract,
		ActivityResultCallback<TResult> callback) =>
		componentActivity.RegisterForActivityResult(contract, callback);

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

		if (!_requestOwners.TryGetValue(launchingActivity, out var requestOwner)
			|| !_activityLaunchers.TryGetValue(launchingActivity, out var launcher))
		{
			Trace.WriteLine("""
			                ActivityForResultRequest is not registered for the launching activity; cancelling the request.
			                Ensure your Activity inherits from ComponentActivity and call Microsoft.Maui.ApplicationModel.Platform.Init(Activity, Bundle) in OnCreate.
			                """);
			return Task.FromCanceled<TResult>(new System.Threading.CancellationToken(true));
		}

		var request = _requestState.BeginRequest(requestOwner);
		if (request.IsFaulted)
			return request;

		try
		{
			launcher.Launch(input);
		}
		catch (Exception ex)
		{
			if (_requestState.TrySetException(requestOwner, ex))
				StopTaskRemovalMonitor(requestOwner);
		}

		return request;
	}

	internal void SaveInstanceState(ComponentActivity componentActivity, Bundle outState)
	{
		if (_requestOwners.TryGetValue(componentActivity, out var requestOwner))
		{
			_requestState.SaveOwner(outState, requestOwner);
			_savedRequestOwners.GetValue(componentActivity, static _ => new object());
		}
	}

	/// <summary>
	/// Cancels any pending request for the specified activity.
	/// This should be called from the activity's OnDestroy() or when the activity is being destroyed
	/// to ensure the pending task is completed rather than hanging indefinitely.
	/// </summary>
	/// <param name="componentActivity">The activity whose pending request should be cancelled.</param>
	internal void CancelPendingRequest(ComponentActivity componentActivity)
	{
		if (_requestOwners.TryGetValue(componentActivity, out var requestOwner))
		{
			if (_requestState.TrySetCanceled(requestOwner))
				StopTaskRemovalMonitor(requestOwner);
		}
	}

	internal void ActivityDestroyed(ComponentActivity componentActivity)
	{
		if (!_requestOwners.TryGetValue(componentActivity, out var requestOwner)
			|| !_requestState.HasPendingRequest(requestOwner))
		{
			return;
		}

		if (componentActivity.IsFinishing
			|| !_savedRequestOwners.TryGetValue(componentActivity, out _)
			|| componentActivity.GetSystemService(global::Android.Content.Context.ActivityService) is not global::Android.App.ActivityManager activityManager
			|| !IsTaskPresent(activityManager, componentActivity.TaskId))
		{
			CancelPendingRequest(componentActivity);
			return;
		}

		// A saved owner can be adopted by a replacement activity, but task removal may
		// happen later without another callback for this already-destroyed instance.
		StartTaskRemovalMonitor(requestOwner, activityManager, componentActivity.TaskId);
	}

	void IActivityForResultRequest.SaveInstanceState(ComponentActivity componentActivity, Bundle outState) =>
		SaveInstanceState(componentActivity, outState);

	void IActivityForResultRequest.ActivityDestroyed(ComponentActivity componentActivity) =>
		ActivityDestroyed(componentActivity);

	void StartTaskRemovalMonitor(
		string requestOwner,
		global::Android.App.ActivityManager activityManager,
		int taskId)
	{
		var cancellation = new System.Threading.CancellationTokenSource();
		if (!_taskRemovalMonitors.TryAdd(requestOwner, cancellation))
		{
			cancellation.Dispose();
			return;
		}

		_ = MonitorTaskPresenceAsync(requestOwner, activityManager, taskId, cancellation);
	}

	async Task MonitorTaskPresenceAsync(
		string requestOwner,
		global::Android.App.ActivityManager activityManager,
		int taskId,
		System.Threading.CancellationTokenSource cancellation)
	{
		try
		{
			while (_requestState.HasPendingRequest(requestOwner))
			{
				await Task.Delay(TaskPresencePollInterval, cancellation.Token).ConfigureAwait(false);
				if (!IsTaskPresent(activityManager, taskId))
				{
					_requestState.TrySetCanceled(requestOwner);
					break;
				}
			}
		}
		catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
		{
		}
		catch (global::Java.Lang.SecurityException ex)
		{
			_requestState.TrySetException(requestOwner, ex);
		}
		finally
		{
			if (_taskRemovalMonitors.TryRemove(
				new KeyValuePair<string, System.Threading.CancellationTokenSource>(requestOwner, cancellation)))
			{
				cancellation.Dispose();
			}
		}
	}

	void StopTaskRemovalMonitor(string requestOwner)
	{
		if (_taskRemovalMonitors.TryRemove(requestOwner, out var cancellation))
		{
			cancellation.Cancel();
			cancellation.Dispose();
		}
	}

	static bool IsTaskPresent(global::Android.App.ActivityManager activityManager, int taskId)
	{
		foreach (var appTask in activityManager.AppTasks ?? [])
		{
			if (appTask.TaskInfo?.TaskId == taskId)
				return true;
		}

		return false;
	}
}

internal sealed class ActivityForResultRequestState<TResult>
{
	readonly string _stateKey;
	readonly ConcurrentDictionary<string, TaskCompletionSource<TResult>> _pendingRequests = new();

	internal ActivityForResultRequestState(string stateKey)
	{
		_stateKey = stateKey;
	}

	internal string RestoreOrCreateOwner(Bundle? savedInstanceState) =>
		savedInstanceState?.GetString(_stateKey) ?? Guid.NewGuid().ToString("N");

	internal void SaveOwner(Bundle outState, string requestOwner) =>
		outState.PutString(_stateKey, requestOwner);

	internal Task<TResult> BeginRequest(string requestOwner)
	{
		var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		if (!_pendingRequests.TryAdd(requestOwner, tcs))
		{
			Trace.WriteLine("ActivityForResultRequest: rejecting overlapping request for the same activity.");
			return Task.FromException<TResult>(
				new InvalidOperationException("An activity result request is already pending for this activity."));
		}

		return tcs.Task;
	}

	internal bool HasPendingRequest(string requestOwner) =>
		_pendingRequests.ContainsKey(requestOwner);

	internal bool TrySetResult(string requestOwner, TResult result) =>
		_pendingRequests.TryRemove(requestOwner, out var tcs) && tcs.TrySetResult(result);

	internal bool TrySetException(string requestOwner, Exception exception) =>
		_pendingRequests.TryRemove(requestOwner, out var tcs) && tcs.TrySetException(exception);

	internal bool TrySetCanceled(string requestOwner) =>
		_pendingRequests.TryRemove(requestOwner, out var tcs) && tcs.TrySetCanceled();
}
