#nullable enable
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Microsoft.Maui.ApplicationModel;
using Xunit;
using ActivityOptionsCompat = AndroidX.Core.App.ActivityOptionsCompat;
using JavaObject = Java.Lang.Object;
using JavaString = Java.Lang.String;
using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Category("ActivityStateManager")]
public class ActivityStateManagerRecreation_Tests
{
	[Fact]
	public async Task PendingRequest_CompletesThroughRecreatedActivityCallback()
	{
		var request = new RecreationActivityForResultRequest();
		var manager = new ActivityStateManagerImplementation(request);
		ActivityResultRecreationState.Begin(manager);

		ActivityResultRecreationActivity? originalActivity = null;
		ActivityResultRecreationActivity? recreatedActivity = null;

		try
		{
			var hostActivity = MauiPlatform.CurrentActivity
				?? throw new InvalidOperationException("The device-test host activity is unavailable.");

			await MainThread.InvokeOnMainThreadAsync(() =>
				hostActivity.StartActivity(new Intent(hostActivity, typeof(ActivityResultRecreationActivity))));

			originalActivity = await ActivityResultRecreationState.FirstActivity
				.WaitAsync(TimeSpan.FromSeconds(15));

			Task<JavaString>? pendingResult = null;
			using var input = new JavaString("recreated-result");
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				pendingResult = request.Launch(originalActivity, input);
			});
			var pending = pendingResult
				?? throw new InvalidOperationException("The activity-result request was not started.");
			var requestCode = await ActivityResultRecreationState.RequestCode
				.WaitAsync(TimeSpan.FromSeconds(15));

			Assert.False(pending.IsCompleted);
			await MainThread.InvokeOnMainThreadAsync(originalActivity.Recreate);

			await ActivityResultRecreationState.OriginalActivityDestroyed
				.WaitAsync(TimeSpan.FromSeconds(15));
			recreatedActivity = await ActivityResultRecreationState.RecreatedActivity
				.WaitAsync(TimeSpan.FromSeconds(15));

			Assert.NotSame(originalActivity, recreatedActivity);
			Assert.False(pending.IsCompleted);

			using var callbackResult = new JavaString("recreated-result");
			await MainThread.InvokeOnMainThreadAsync(() =>
				recreatedActivity.ResultRegistry.Deliver(requestCode, callbackResult));

			var result = await pending.WaitAsync(TimeSpan.FromSeconds(15));
			Assert.Same(callbackResult, result);
			Assert.Equal("recreated-result", result.ToString());
		}
		finally
		{
			recreatedActivity ??= manager.GetCurrentActivity() as ActivityResultRecreationActivity;
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (recreatedActivity is { IsFinishing: false })
					recreatedActivity.Finish();
				if (originalActivity is { IsDestroyed: false, IsFinishing: false })
					originalActivity.Finish();
			});

			manager.Dispose();
			ActivityResultRecreationState.Reset();
		}
	}

	[Fact]
	public async Task PendingRequest_FaultsWhenTaskPresenceCheckIsDenied()
	{
		var request = new RecreationActivityForResultRequest();
		var manager = new ActivityStateManagerImplementation(request);
		ActivityResultRecreationState.Begin(manager);

		ActivityResultRecreationActivity? activity = null;

		try
		{
			var hostActivity = MauiPlatform.CurrentActivity
				?? throw new InvalidOperationException("The device-test host activity is unavailable.");

			await MainThread.InvokeOnMainThreadAsync(() =>
				hostActivity.StartActivity(new Intent(hostActivity, typeof(ActivityResultRecreationActivity))));
			activity = await ActivityResultRecreationState.FirstActivity
				.WaitAsync(TimeSpan.FromSeconds(15));

			Task<JavaString>? pendingResult = null;
			using var input = new JavaString("security-exception");
			using var savedState = new Bundle();
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				pendingResult = request.Launch(activity, input);
				request.SaveInstanceState(activity, savedState);
				request.DenyTaskPresenceCheck = true;
				request.ActivityDestroyed(activity);
			});
			var pending = pendingResult
				?? throw new InvalidOperationException("The activity-result request was not started.");

			var exception = await Assert.ThrowsAsync<global::Java.Lang.SecurityException>(
				() => pending.WaitAsync(TimeSpan.FromSeconds(15)));
			Assert.Contains("task presence denied", exception.Message, StringComparison.Ordinal);
		}
		finally
		{
			activity ??= manager.GetCurrentActivity() as ActivityResultRecreationActivity;
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (activity is { IsDestroyed: false, IsFinishing: false })
					activity.Finish();
			});

			manager.Dispose();
			ActivityResultRecreationState.Reset();
		}
	}

	[Fact]
	public async Task PendingRequest_IsCanceledWhenOwningTaskIsRemoved()
	{
		var request = new RecreationActivityForResultRequest();
		var manager = new ActivityStateManagerImplementation(request);
		ActivityResultRecreationState.Begin(manager);

		ActivityResultRecreationActivity? activity = null;

		try
		{
			var hostActivity = MauiPlatform.CurrentActivity
				?? throw new InvalidOperationException("The device-test host activity is unavailable.");
			var intent = new Intent(hostActivity, typeof(ActivityResultRecreationActivity))
				.AddFlags(ActivityFlags.NewDocument | ActivityFlags.MultipleTask);

			await MainThread.InvokeOnMainThreadAsync(() => hostActivity.StartActivity(intent));
			activity = await ActivityResultRecreationState.FirstActivity
				.WaitAsync(TimeSpan.FromSeconds(15));

			Task<JavaString>? pendingResult = null;
			using var input = new JavaString("removed-task");
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				pendingResult = request.Launch(activity, input);
			});
			var pending = pendingResult
				?? throw new InvalidOperationException("The activity-result request was not started.");
			await ActivityResultRecreationState.RequestCode.WaitAsync(TimeSpan.FromSeconds(15));

			var activityManager = activity.GetSystemService(Context.ActivityService) as ActivityManager
				?? throw new InvalidOperationException("The Android activity manager is unavailable.");
			var appTask = FindTask(activityManager, activity.TaskId)
				?? throw new InvalidOperationException("The activity's Android task was not found.");

			await MainThread.InvokeOnMainThreadAsync(appTask.FinishAndRemoveTask);
			await ActivityResultRecreationState.OriginalActivityDestroyed
				.WaitAsync(TimeSpan.FromSeconds(15));

			await Assert.ThrowsAnyAsync<System.OperationCanceledException>(
				() => pending.WaitAsync(TimeSpan.FromSeconds(15)));
		}
		finally
		{
			activity ??= manager.GetCurrentActivity() as ActivityResultRecreationActivity;
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (activity is { IsDestroyed: false, IsFinishing: false })
					activity.FinishAndRemoveTask();
			});

			manager.Dispose();
			ActivityResultRecreationState.Reset();
		}
	}

	static ActivityManager.AppTask? FindTask(ActivityManager activityManager, int taskId)
	{
		foreach (var appTask in activityManager.AppTasks ?? [])
		{
			if (appTask.TaskInfo?.TaskId == taskId)
				return appTask;
		}

		return null;
	}
}

sealed class RecreationActivityForResultRequest
	: ActivityForResultRequest<RecreationActivityResultContract, JavaString>
{
	internal bool DenyTaskPresenceCheck { get; set; }

	protected override ActivityResultLauncher RegisterForActivityResult(
		ComponentActivity componentActivity,
		RecreationActivityResultContract contract,
		ActivityResultCallback<JavaString> callback)
	{
		var activity = Assert.IsType<ActivityResultRecreationActivity>(componentActivity);
		return activity.RegisterForActivityResult(contract, activity.ResultRegistry, callback);
	}

	protected override bool IsTaskPresent(ActivityManager activityManager, int taskId) =>
		DenyTaskPresenceCheck
			? throw new global::Java.Lang.SecurityException("task presence denied")
			: base.IsTaskPresent(activityManager, taskId);
}

sealed class RecreationActivityResultContract : ActivityResultContract
{
	public override Intent CreateIntent(Context context, JavaObject? input) =>
		new Intent();

	public override JavaObject ParseResult(int resultCode, Intent? intent) =>
		new JavaString();
}

sealed class RecreationActivityResultRegistry : ActivityResultRegistry
{
	public override void OnLaunch(
		int requestCode,
		ActivityResultContract contract,
		JavaObject? input,
		ActivityOptionsCompat? options) =>
		ActivityResultRecreationState.OnRequestLaunched(requestCode);

	internal void Deliver(int requestCode, JavaString result)
	{
		if (!DispatchResult(requestCode, result))
			throw new InvalidOperationException("The recreated AndroidX activity-result registry rejected the result.");
	}
}

[Activity(Theme = "@android:style/Theme.Material.Light.NoActionBar", Exported = false)]
public sealed class ActivityResultRecreationActivity : ComponentActivity
{
	const string RegistryStateKey = "ActivityStateManagerRecreation.Registry";

	internal RecreationActivityResultRegistry ResultRegistry { get; } = new();

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		ResultRegistry.OnRestoreInstanceState(savedInstanceState?.GetBundle(RegistryStateKey));
		MauiPlatform.Init(this, savedInstanceState);
		ActivityResultRecreationState.Manager?.Init(this, savedInstanceState);
	}

	protected override void OnResume()
	{
		base.OnResume();
		ActivityResultRecreationState.OnActivityResumed(this);
	}

	protected override void OnSaveInstanceState(Bundle outState)
	{
		var registryState = new Bundle();
		ResultRegistry.OnSaveInstanceState(registryState);
		outState.PutBundle(RegistryStateKey, registryState);

		base.OnSaveInstanceState(outState);
	}

	protected override void OnDestroy()
	{
		ActivityResultRecreationState.OnActivityDestroyed(this);
		base.OnDestroy();
	}
}

static class ActivityResultRecreationState
{
	static TaskCompletionSource<ActivityResultRecreationActivity> _firstActivity = CreateActivitySource();
	static TaskCompletionSource<ActivityResultRecreationActivity> _recreatedActivity = CreateActivitySource();
	static TaskCompletionSource _originalActivityDestroyed = CreateSignalSource();
	static TaskCompletionSource<int> _requestCode = CreateRequestCodeSource();

	internal static ActivityStateManagerImplementation? Manager { get; private set; }
	internal static Task<ActivityResultRecreationActivity> FirstActivity => _firstActivity.Task;
	internal static Task<ActivityResultRecreationActivity> RecreatedActivity => _recreatedActivity.Task;
	internal static Task OriginalActivityDestroyed => _originalActivityDestroyed.Task;
	internal static Task<int> RequestCode => _requestCode.Task;

	internal static void Begin(ActivityStateManagerImplementation manager)
	{
		Reset();
		Manager = manager;
	}

	internal static void OnActivityResumed(ActivityResultRecreationActivity activity)
	{
		if (!_firstActivity.TrySetResult(activity))
			_recreatedActivity.TrySetResult(activity);
	}

	internal static void OnActivityDestroyed(ActivityResultRecreationActivity activity)
	{
		if (_firstActivity.Task.IsCompletedSuccessfully
			&& ReferenceEquals(_firstActivity.Task.Result, activity))
		{
			_originalActivityDestroyed.TrySetResult();
		}
	}

	internal static void OnRequestLaunched(int requestCode) =>
		_requestCode.TrySetResult(requestCode);

	internal static void Reset()
	{
		Manager = null;
		_firstActivity = CreateActivitySource();
		_recreatedActivity = CreateActivitySource();
		_originalActivityDestroyed = CreateSignalSource();
		_requestCode = CreateRequestCodeSource();
	}

	static TaskCompletionSource<ActivityResultRecreationActivity> CreateActivitySource() =>
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	static TaskCompletionSource CreateSignalSource() =>
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	static TaskCompletionSource<int> CreateRequestCodeSource() =>
		new(TaskCreationOptions.RunContinuationsAsynchronously);
}
