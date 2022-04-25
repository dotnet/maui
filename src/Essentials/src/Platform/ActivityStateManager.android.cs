#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IActivityStateManager
	{
		void Init(Application application);

		void Init(Activity activity, Bundle? bundle);

		Activity? GetCurrentActivity();

		event EventHandler<ActivityStateChangedEventArgs> ActivityStateChanged;

		Task<Activity> WaitForActivityAsync(CancellationToken cancelToken = default);
	}

	public static class ActivityStateManager
	{
		static IActivityStateManager? defaultImplementation;

		public static IActivityStateManager Default =>
			defaultImplementation ??= new ActivityStateManagerImplementation();

		internal static void SetDefault(IActivityStateManager? implementation) =>
			defaultImplementation = implementation;
	}

	class ActivityStateManagerImplementation : IActivityStateManager
	{
		ActivityLifecycleContextListener? lifecycleListener;

		public Activity? GetCurrentActivity() => lifecycleListener?.Activity;

		public event EventHandler<ActivityStateChangedEventArgs>? ActivityStateChanged;

		public void Init(Application application)
		{
			lifecycleListener = new ActivityLifecycleContextListener(OnActivityStateChanged);
			application.RegisterActivityLifecycleCallbacks(lifecycleListener);
		}

		public void Init(Activity activity, Bundle? bundle)
		{
			if (activity.Application is not Application application)
				throw new InvalidOperationException("Activity was not attached to an application.");

			Init(application);
			lifecycleListener!.Activity = activity;
		}

		public async Task<Activity> WaitForActivityAsync(CancellationToken cancelToken = default)
		{
			if (GetCurrentActivity() is Activity activity)
				return activity;

			var tcs = new TaskCompletionSource<Activity>();

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

			void handler(object? sender, ActivityStateChangedEventArgs e)
			{
				if (e.State == ActivityState.Created || e.State == ActivityState.Resumed)
					tcs.TrySetResult(e.Activity);
			}
		}

		void OnActivityStateChanged(Activity activity, ActivityState ev)
			=> ActivityStateChanged?.Invoke(null, new ActivityStateChangedEventArgs(activity, ev));
	}

	static class ActivityStateManagerExtensions
	{
		public static Activity? GetCurrentActivity(this IActivityStateManager manager, bool throwOnNull)
		{
			var activity = manager.GetCurrentActivity();
			if (throwOnNull && activity == null)
				throw new NullReferenceException("The current Activity can not be detected. Ensure that you have called Init in your Activity or Application class.");

			return activity;
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
		readonly Action<Activity, ActivityState> _onActivityStateChanged;
		readonly WeakReference<Activity?> _currentActivity = new(null);

		public ActivityLifecycleContextListener(Action<Activity, ActivityState> onActivityStateChanged)
		{
			_onActivityStateChanged = onActivityStateChanged;
		}

		public Context Context =>
			Activity ?? Application.Context;

		public Activity? Activity
		{
			get => _currentActivity.TryGetTarget(out var a) ? a : null;
			set => _currentActivity.SetTarget(value);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Bundle? savedInstanceState)
		{
			Activity = activity;
			_onActivityStateChanged(activity, ActivityState.Created);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity) =>
			_onActivityStateChanged(activity, ActivityState.Destroyed);

		void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity)
		{
			Activity = activity;
			_onActivityStateChanged(activity, ActivityState.Paused);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity)
		{
			Activity = activity;
			_onActivityStateChanged(activity, ActivityState.Resumed);
		}

		void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Bundle outState) =>
			_onActivityStateChanged(activity, ActivityState.SaveInstanceState);

		void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity) =>
			_onActivityStateChanged(activity, ActivityState.Started);

		void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity) =>
			_onActivityStateChanged(activity, ActivityState.Stopped);
	}
}
