using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Bundle = Android.OS.Bundle;
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
/// Pending requests use an owner identifier saved in the activity instance state. A callback
/// registered by a recreated activity can therefore resolve the task started by the previous
/// activity instance, including recreation that does not retain the ViewModelStore.
/// </para>
/// </remarks>
internal abstract class ActivityForResultRequest<TContract, TResult>
	where TContract : ActivityResultContract, new()
	where TResult : JavaObject
{
	static readonly string RequestOwnerStateKey =
		$"Microsoft.Maui.ApplicationModel.ActivityForResultRequest.{typeof(TContract).FullName}";

	// Tracks one ActivityResultLauncher per ComponentActivity instance.
	// ConditionalWeakTable holds weak references to keys, so entries are automatically
	// eligible for collection when the activity is no longer referenced.
	readonly ConditionalWeakTable<ComponentActivity, ActivityResultLauncher> _activityLaunchers = new();

	readonly ConditionalWeakTable<ComponentActivity, string> _requestOwners = new();
	readonly ActivityForResultRequestState<TResult> _requestState = new(RequestOwnerStateKey);

	/// <summary>
	/// Registers this request to start an activity for a result.
	/// Each <see cref="ComponentActivity"/> instance receives its own launcher so child
	/// activities can use MediaPicker independently of the main activity.
	/// </summary>
	/// <param name="componentActivity">The component activity to register the request with.</param>
	/// <param name="savedInstanceState">State restored for a recreated activity, if available.</param>
	public void Register(ComponentActivity componentActivity, Bundle savedInstanceState)
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
		var callback = new ActivityResultCallback<TResult>(result =>
		{
			_requestState.TrySetResult(requestOwner, result);
		});

		var launcher = componentActivity.RegisterForActivityResult(contract, callback);
		_requestOwners.Add(componentActivity, requestOwner);
		_activityLaunchers.Add(componentActivity, launcher);
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
			_requestState.TrySetException(requestOwner, ex);
		}

		return request;
	}

	internal void SaveInstanceState(ComponentActivity componentActivity, Bundle outState)
	{
		if (_requestOwners.TryGetValue(componentActivity, out var requestOwner))
			_requestState.SaveOwner(outState, requestOwner);
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
			_requestState.TrySetCanceled(requestOwner);
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

	internal string RestoreOrCreateOwner(Bundle savedInstanceState) =>
		savedInstanceState?.GetString(_stateKey) ?? Guid.NewGuid().ToString("N");

	internal void SaveOwner(Bundle outState, string requestOwner) =>
		outState.PutString(_stateKey, requestOwner);

	internal Task<TResult> BeginRequest(string requestOwner)
	{
		var tcs = new TaskCompletionSource<TResult>();
		if (!_pendingRequests.TryAdd(requestOwner, tcs))
		{
			Trace.WriteLine("ActivityForResultRequest: rejecting overlapping request for the same activity.");
			return Task.FromException<TResult>(
				new InvalidOperationException("An activity result request is already pending for this activity."));
		}

		return tcs.Task;
	}

	internal bool TrySetResult(string requestOwner, TResult result) =>
		_pendingRequests.TryRemove(requestOwner, out var tcs) && tcs.TrySetResult(result);

	internal bool TrySetException(string requestOwner, Exception exception) =>
		_pendingRequests.TryRemove(requestOwner, out var tcs) && tcs.TrySetException(exception);

	internal bool TrySetCanceled(string requestOwner) =>
		_pendingRequests.TryRemove(requestOwner, out var tcs) && tcs.TrySetCanceled();
}
