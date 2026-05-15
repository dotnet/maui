using System;
using System.Diagnostics;
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
/// A launch request is one in-process call to <see cref="Launch{T}(T)"/> that is waiting for its AndroidX result.
/// AndroidX may also replay a result after activity or process recreation, after the original launch request is gone.
/// </para>
/// This must be unconditionally registered every time our activity is created.
/// </remarks>
internal abstract class ActivityForResultRequest<TContract, TResult>
	where TContract : ActivityResultContract, new()
	where TResult : JavaObject
{
	// Protects the active launch completion source so a launch, result callback, and launch failure
	// cannot race to overwrite or clear the in-process request state.
	readonly Lock activeLaunchLock = new();
	ActivityResultLauncher launcher;
	TaskCompletionSource<TResult> activeLaunchCompletionSource = null;
	WeakReference<ComponentActivity> registeredActivity = null;

	/// <summary>
	/// Gets a value indicating whether the request is registered.
	/// </summary>
	protected bool IsRegistered => launcher is not null;

	/// <summary>
	/// Registers this request to start an activity for a result.
	/// </summary>
	/// <param name="componentActivity">The component activity to register the request with.</param>
	public void Register(ComponentActivity componentActivity)
	{
		// Only register if we don't have a valid registration already
		// This prevents temporary activities from invalidating the launcher registered with the main activity
		if (registeredActivity?.TryGetTarget(out var existingActivity) == true &&
			!existingActivity.IsDestroyed && !existingActivity.IsFinishing)
		{
			return;
		}

		var contract = new TContract();
		var callback = new ActivityResultCallback<TResult>(HandleActivityResult);

		launcher = componentActivity.RegisterForActivityResult(contract, callback);
		registeredActivity = new WeakReference<ComponentActivity>(componentActivity);
	}

	/// <summary>
	/// Routes an AndroidX activity result to either the active launch task or orphaned-result handling.
	/// </summary>
	/// <param name="result">The activity result.</param>
	/// <remarks>
	/// An orphaned result is a pending AndroidX result replayed after activity or process recreation,
	/// when the launch task that originally requested the result no longer exists in this process.
	/// </remarks>
	protected void HandleActivityResult(TResult result)
	{
		var completionSource = TakeActiveLaunchCompletionSource();
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

	/// <summary>
	/// Takes the active task completion source if this result belongs to a launch request in this process.
	/// </summary>
	/// <returns>The active launch task completion source, or <see langword="null"/> when the result is orphaned.</returns>
	TaskCompletionSource<TResult> TakeActiveLaunchCompletionSource()
	{
		lock (activeLaunchLock)
		{
			var completionSource = activeLaunchCompletionSource;
			activeLaunchCompletionSource = null;
			return completionSource;
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
		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		lock (activeLaunchLock)
		{
			if (activeLaunchCompletionSource is not null)
			{
				return Task.FromException<TResult>(new InvalidOperationException("An activity result request is already in progress."));
			}

			activeLaunchCompletionSource = completionSource;
		}

		if (!IsRegistered)
		{
			Trace.WriteLine("""
			                ActivityForResultRequest is not registered; cancelling the request.
			                Ensure your Activity inherits from ComponentActivity and call Microsoft.Maui.ApplicationModel.Platform.Init(Activity, Bundle) in OnCreate.
			                """);
			ClearActiveLaunchCompletionSource(completionSource);
			completionSource.SetCanceled();
			return completionSource.Task;
		}

		try
		{
			launcher.Launch(input);
		}
		catch (Exception ex)
		{
			ClearActiveLaunchCompletionSource(completionSource);
			completionSource.SetException(ex);
		}

		return completionSource.Task;
	}

	void ClearActiveLaunchCompletionSource(TaskCompletionSource<TResult> completionSource)
	{
		lock (activeLaunchLock)
		{
			if (ReferenceEquals(activeLaunchCompletionSource, completionSource))
				activeLaunchCompletionSource = null;
		}
	}
}