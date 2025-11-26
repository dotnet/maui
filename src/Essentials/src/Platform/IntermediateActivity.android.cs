#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Microsoft.Maui.ApplicationModel
{
	[Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, Exported = false)]
	class IntermediateActivity : Activity
	{
		const string launchedExtra = "launched";
		const string actualIntentExtra = "actual_intent";
		const string guidExtra = "guid";
		const string requestCodeExtra = "request_code";

		static readonly ConcurrentDictionary<string, IntermediateTask> pendingTasks = new();

		bool launched;
		Intent? actualIntent;
		string? guid;
		int requestCode;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var extras = savedInstanceState ?? Intent?.Extras;

			// read the values
			launched = extras?.GetBoolean(launchedExtra, false) ?? false;

			if (OperatingSystem.IsAndroidVersionAtLeast(33))
				actualIntent = extras?.GetParcelable(actualIntentExtra, Java.Lang.Class.FromType(typeof(Intent))) as Intent;
			else
				actualIntent = extras?.GetParcelable(actualIntentExtra) as Intent;

			guid = extras?.GetString(guidExtra);
			requestCode = extras?.GetInt(requestCodeExtra, -1) ?? -1;

			if (GetIntermediateTask(guid) is IntermediateTask task)
			{
				task.OnCreate?.Invoke(actualIntent!);
			}

			// if this is the first time, lauch the real activity
			if (!launched)
				StartActivityForResult(actualIntent, requestCode);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			// make sure we mark this activity as launched
			outState.PutBoolean(launchedExtra, true);

			// save the values
			outState.PutParcelable(actualIntentExtra, actualIntent);
			outState.PutString(guidExtra, guid);
			outState.PutInt(requestCodeExtra, requestCode);

			base.OnSaveInstanceState(outState);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			var task = GetIntermediateTask(guid, true);

			Finish();

			if (task is null)
				return;

			if (resultCode == Result.Canceled)
			{
				task.TaskCompletionSource.TrySetCanceled();
			}
			else
			{
				try
				{
					data ??= new Intent();

					task.OnResult?.Invoke(data);

					task.TaskCompletionSource.TrySetResult(data);
				}
				catch (Exception ex)
				{
					task.TaskCompletionSource.TrySetException(ex);
				}
			}
		}

		public static Task<Intent> StartAsync(Intent intent, int requestCode, Action<Intent>? onCreate = null, Action<Intent>? onResult = null)
		{
			// make sure we have the activity
			var activity = ActivityStateManager.Default.GetCurrentActivity(true)!;

			// create a new task
			var data = new IntermediateTask(onCreate, onResult);
			pendingTasks[data.Id] = data;

			// create the intermediate intent, and add the real intent to it
			var intermediateIntent = new Intent(activity, typeof(IntermediateActivity));
			intermediateIntent.PutExtra(actualIntentExtra, intent);
			intermediateIntent.PutExtra(guidExtra, data.Id);
			intermediateIntent.PutExtra(requestCodeExtra, requestCode);

			// start the intermediate activity
			activity.StartActivityForResult(intermediateIntent, requestCode);

			return data.TaskCompletionSource.Task;
		}

		static IntermediateTask? GetIntermediateTask(string? guid, bool remove = false)
		{
			if (string.IsNullOrEmpty(guid))
				return null;

			if (remove)
			{
				pendingTasks.TryRemove(guid, out var removedTask);
				return removedTask;
			}

			pendingTasks.TryGetValue(guid, out var task);
			return task;
		}

		class IntermediateTask
		{
			public IntermediateTask(Action<Intent>? onCreate, Action<Intent>? onResult)
			{
				Id = Guid.NewGuid().ToString();
				TaskCompletionSource = new TaskCompletionSource<Intent>();

				OnCreate = onCreate;
				OnResult = onResult;
			}

			public string Id { get; }

			public TaskCompletionSource<Intent> TaskCompletionSource { get; }

			public Action<Intent>? OnCreate { get; }

			public Action<Intent>? OnResult { get; }
		}
	}
}
