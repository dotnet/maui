using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Views;
using AndroidIntent = Android.Content.Intent;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Essentials
{
	public static partial class Platform
	{
		static ActivityLifecycleContextListener lifecycleListener;

		public static Context AppContext => Application.Context;

		public static Activity CurrentActivity => lifecycleListener?.Activity;

		public static event EventHandler<ActivityStateChangedEventArgs> ActivityStateChanged;

		internal const int requestCodeFilePicker = 11001;
		internal const int requestCodeMediaPicker = 11002;
		internal const int requestCodeMediaCapture = 11003;
		internal const int requestCodePickContact = 11004;

		internal const int requestCodeStart = 12000;

		static int requestCode = requestCodeStart;

		internal static int NextRequestCode()
		{
			if (++requestCode >= 12999)
				requestCode = requestCodeStart;

			return requestCode;
		}

		internal static void OnActivityStateChanged(Activity activity, ActivityState ev)
			=> ActivityStateChanged?.Invoke(null, new ActivityStateChangedEventArgs(activity, ev));

		public static async Task<Activity> WaitForActivityAsync(CancellationToken cancelToken = default)
		{
			if (CurrentActivity != null)
				return CurrentActivity;

			var tcs = new TaskCompletionSource<Activity>();
			var handler = new EventHandler<ActivityStateChangedEventArgs>((sender, args) =>
			{
				if (args.State == ActivityState.Created || args.State == ActivityState.Resumed)
					tcs.TrySetResult(args.Activity);
			});

			try
			{
				using (cancelToken.Register(() => tcs.TrySetCanceled()))
				{
					ActivityStateChanged += handler;
					return await tcs.Task.ConfigureAwait(false);
				}
			}
			finally
			{
				ActivityStateChanged -= handler;
			}
		}

		internal static Activity GetCurrentActivity(bool throwOnNull)
		{
			var activity = lifecycleListener?.Activity;
			if (throwOnNull && activity == null)
				throw new NullReferenceException("The current Activity can not be detected. Ensure that you have called Init in your Activity or Application class.");

			return activity;
		}

		public static void Init(Application application)
		{
			lifecycleListener = new ActivityLifecycleContextListener();
			application.RegisterActivityLifecycleCallbacks(lifecycleListener);
		}

		public static void Init(Activity activity, Bundle bundle)
		{
			Init(activity.Application);
			lifecycleListener.Activity = activity;
		}

		public static void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) =>
			Permissions.OnRequestPermissionsResult(requestCode, permissions, grantResults);

		public static void OnNewIntent(AndroidIntent intent)
			=> CheckAppActions(intent);

		public static void OnResume(Activity activity = null)
		{
			if (activity != null)
				CheckAppActions(activity.Intent);
		}

		static void CheckAppActions(AndroidIntent intent)
		{
			if (intent?.Action == Intent.ActionAppAction)
			{
				var appAction = intent.ToAppAction();

				if (!string.IsNullOrEmpty(appAction?.Id))
					AppActions.InvokeOnAppAction(Platform.CurrentActivity, appAction);
			}
		}

		internal static bool HasSystemFeature(string systemFeature)
		{
			var packageManager = AppContext.PackageManager;
			foreach (var feature in packageManager.GetSystemAvailableFeatures())
			{
				if (feature?.Name?.Equals(systemFeature, StringComparison.OrdinalIgnoreCase) ?? false)
					return true;
			}
			return false;
		}

		internal static bool IsIntentSupported(AndroidIntent intent) =>
			intent.ResolveActivity(AppContext.PackageManager) != null;

		internal static bool IsIntentSupported(AndroidIntent intent, string expectedPackageName) =>
			intent.ResolveActivity(AppContext.PackageManager) is ComponentName c && c.PackageName == expectedPackageName;

		internal static AndroidUri GetShareableFileUri(FileBase file)
		{
			Java.IO.File sharedFile;
			if (FileProvider.IsFileInPublicLocation(file.FullPath))
			{
				// we are sharing a file in a "shared/public" location
				sharedFile = new Java.IO.File(file.FullPath);
			}
			else
			{
				var root = FileProvider.GetTemporaryRootDirectory();

				var tmpFile = FileSystem.GetEssentialsTemporaryFile(root, file.FileName);

				System.IO.File.Copy(file.FullPath, tmpFile.CanonicalPath);

				sharedFile = tmpFile;
			}

			// create the uri, if N use file provider
			if (HasApiLevelN)
			{
				return FileProvider.GetUriForFile(sharedFile);
			}

			// use the shared file path created
			return AndroidUri.FromFile(sharedFile);
		}

		internal static bool HasApiLevelKitKat => HasApiLevel(BuildVersionCodes.Kitkat);

		internal static bool HasApiLevelN => HasApiLevel(24);

		internal static bool HasApiLevelNMr1 => HasApiLevel(25);

		internal static bool HasApiLevelO => HasApiLevel(26);

		internal static bool HasApiLevelQ => HasApiLevel(29);

		static int? sdkInt;

		internal static int SdkInt
			=> sdkInt ??= (int)Build.VERSION.SdkInt;

		internal static bool HasApiLevel(BuildVersionCodes versionCode) =>
			SdkInt >= (int)versionCode;

		internal static bool HasApiLevel(int apiLevel) =>
			SdkInt >= apiLevel;

		internal static CameraManager CameraManager =>
			AppContext.GetSystemService(Context.CameraService) as CameraManager;

		internal static ConnectivityManager ConnectivityManager =>
			AppContext.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

		internal static Vibrator Vibrator =>
			AppContext.GetSystemService(Context.VibratorService) as Vibrator;

		internal static WifiManager WifiManager =>
			AppContext.GetSystemService(Context.WifiService) as WifiManager;

		internal static SensorManager SensorManager =>
			AppContext.GetSystemService(Context.SensorService) as SensorManager;

		internal static ClipboardManager ClipboardManager =>
			AppContext.GetSystemService(Context.ClipboardService) as ClipboardManager;

		internal static LocationManager LocationManager =>
			AppContext.GetSystemService(Context.LocationService) as LocationManager;

		internal static PowerManager PowerManager =>
			AppContext.GetSystemService(Context.PowerService) as PowerManager;

#if __ANDROID_25__
        internal static ShortcutManager ShortcutManager =>
            AppContext.GetSystemService(Context.ShortcutService) as ShortcutManager;
#endif

		internal static IWindowManager WindowManager =>
			AppContext.GetSystemService(Context.WindowService) as IWindowManager;

		internal static ContentResolver ContentResolver =>
			AppContext.ContentResolver;

		internal static Java.Util.Locale GetLocale()
		{
			var resources = AppContext.Resources;
			var config = resources.Configuration;
#if __ANDROID_24__
            if (HasApiLevelN)
                return config.Locales.Get(0);
#endif

#pragma warning disable CS0618 // Type or member is obsolete
			return config.Locale;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		internal static void SetLocale(Java.Util.Locale locale)
		{
			Java.Util.Locale.Default = locale;
			var resources = AppContext.Resources;
			var config = resources.Configuration;

#if __ANDROID_24__
            if (HasApiLevelN)
                config.SetLocale(locale);
            else
#endif
#pragma warning disable CS0618 // Type or member is obsolete
			config.Locale = locale;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			resources.UpdateConfiguration(config, resources.DisplayMetrics);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public static class Intent
		{
			public const string ActionAppAction = "ACTION_XE_APP_ACTION";
		}
	}

	public enum ActivityState
	{
		Created,
		Resumed,
		Paused,
		Destroyed,
		SaveInstanceState,
		Started,
		Stopped
	}

	public class ActivityStateChangedEventArgs : EventArgs
	{
		internal ActivityStateChangedEventArgs(Activity activity, ActivityState ev)
		{
			State = ev;
			Activity = activity;
		}

		public ActivityState State { get; }

		public Activity Activity { get; }
	}

	class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		WeakReference<Activity> currentActivity = new WeakReference<Activity>(null);

		internal Context Context =>
			Activity ?? Application.Context;

		internal Activity Activity
		{
			get => currentActivity.TryGetTarget(out var a) ? a : null;
			set => currentActivity.SetTarget(value);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Bundle savedInstanceState)
		{
			Activity = activity;
			Platform.OnActivityStateChanged(activity, ActivityState.Created);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity) =>
			Platform.OnActivityStateChanged(activity, ActivityState.Destroyed);

		void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity)
		{
			Activity = activity;
			Platform.OnActivityStateChanged(activity, ActivityState.Paused);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity)
		{
			Activity = activity;
			Platform.OnActivityStateChanged(activity, ActivityState.Resumed);
		}

		void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Bundle outState) =>
			Platform.OnActivityStateChanged(activity, ActivityState.SaveInstanceState);

		void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity) =>
			Platform.OnActivityStateChanged(activity, ActivityState.Started);

		void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity) =>
			Platform.OnActivityStateChanged(activity, ActivityState.Stopped);
	}

	[Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
	class IntermediateActivity : Activity
	{
		const string launchedExtra = "launched";
		const string actualIntentExtra = "actual_intent";
		const string guidExtra = "guid";
		const string requestCodeExtra = "request_code";

		static readonly ConcurrentDictionary<string, IntermediateTask> pendingTasks =
			new ConcurrentDictionary<string, IntermediateTask>();

		bool launched;
		Intent actualIntent;
		string guid;
		int requestCode;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var extras = savedInstanceState ?? Intent.Extras;

			// read the values
			launched = extras.GetBoolean(launchedExtra, false);
			actualIntent = extras.GetParcelable(actualIntentExtra) as Intent;
			guid = extras.GetString(guidExtra);
			requestCode = extras.GetInt(requestCodeExtra, -1);

			if (GetIntermediateTask(guid) is IntermediateTask task)
			{
				task.OnCreate?.Invoke(actualIntent);
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

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// we have a valid GUID, so handle the task
			if (GetIntermediateTask(guid, true) is IntermediateTask task)
			{
				if (resultCode == Result.Canceled)
				{
					task.TaskCompletionSource.TrySetCanceled();
				}
				else
				{
					try
					{
						data ??= new AndroidIntent();

						task.OnResult?.Invoke(data);

						task.TaskCompletionSource.TrySetResult(data);
					}
					catch (Exception ex)
					{
						task.TaskCompletionSource.TrySetException(ex);
					}
				}
			}

			// close the intermediate activity
			Finish();
		}

		public static Task<Intent> StartAsync(Intent intent, int requestCode, Action<Intent> onCreate = null, Action<Intent> onResult = null)
		{
			// make sure we have the activity
			var activity = Platform.GetCurrentActivity(true);

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

		static IntermediateTask GetIntermediateTask(string guid, bool remove = false)
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
			public IntermediateTask(Action<Intent> onCreate, Action<AndroidIntent> onResult)
			{
				Id = Guid.NewGuid().ToString();
				TaskCompletionSource = new TaskCompletionSource<Intent>();

				OnCreate = onCreate;
				OnResult = onResult;
			}

			public string Id { get; }

			public TaskCompletionSource<Intent> TaskCompletionSource { get; }

			public Action<Intent> OnCreate { get; }

			public Action<AndroidIntent> OnResult { get; }
		}
	}
}
