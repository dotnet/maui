#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Activity;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Represents a manager object that can handle <see cref="Activity"/> states.
	/// </summary>
	public interface IActivityStateManager
	{
		/// <summary>
		/// Initializes the <see cref="ActivityStateManager"/> for the given <see cref="global::Android.App.Application"/>.
		/// </summary>
		/// <param name="application">The <see cref="global::Android.App.Application"/> to use for initialization.</param>
		void Init(Application application);

		/// <summary>
		/// Initializes the <see cref="ActivityStateManager"/> for the given <see cref="Activity"/> and <see cref="Bundle"/>.
		/// </summary>
		/// <param name="activity">The <see cref="Activity"/> to use for initialization.</param>
		/// <param name="bundle">The <see cref="Bundle"/> to use for initialization.</param>
		void Init(Activity activity, Bundle? bundle);

		/// <summary>
		/// Gets the <see cref="Activity"/> object that represents the application's current activity.
		/// </summary>
		Activity? GetCurrentActivity();

		/// <summary>
		/// Occurs when the state of an activity of this application changes.
		/// </summary>
		event EventHandler<ActivityStateChangedEventArgs> ActivityStateChanged;

		/// <summary>
		/// Waits for a <see cref="Activity"/> to be created or resumed.
		/// </summary>
		/// <param name="cancelToken">A token that can be used for cancelling the operation.</param>
		/// <returns>The application's current <see cref="Activity"/> or the <see cref="Activity"/> that has been created or resumed.</returns>
		Task<Activity> WaitForActivityAsync(CancellationToken cancelToken = default);
	}

	/// <summary>
	/// Represents a manager object that can handle <see cref="Activity"/> states.
	/// </summary>
	public static class ActivityStateManager
	{
		static IActivityStateManager? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
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

			if (activity is ComponentActivity componentActivity && MediaPickerImplementation.IsPhotoPickerAvailable)
			{
				PickVisualMediaForResult.Instance.Register(componentActivity);
				PickMultipleVisualMediaForResult.Instance.Register(componentActivity);
			}

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
		/// <summary>
		/// Gets the <see cref="Activity"/> object that represents the application's current activity.
		/// </summary>
		/// <param name="manager">The object to invoke this method on.</param>
		/// <param name="throwOnNull">Throws an exception if no current <see cref="Activity"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see langword="null"/>.</param>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="Activity"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
		public static Activity? GetCurrentActivity(this IActivityStateManager manager, bool throwOnNull)
		{
			var activity = manager.GetCurrentActivity();
			if (throwOnNull && activity == null)
				throw new NullReferenceException("The current Activity cannot be detected. Ensure that you have called Init in your Activity or Application class.");

			return activity;
		}
	}

	/// <summary>
	/// Represents states that a <see cref="Activity"/> can have.
	/// </summary>
	public enum ActivityState
	{
		/// <summary>The activity is created.</summary>
		Created,

		/// <summary>The activity is resumed.</summary>
		Resumed,

		/// <summary>The activity is paused.</summary>
		Paused,

		/// <summary>The activity is destroyed.</summary>
		Destroyed,

		/// <summary>The activity saving the instance state.</summary>
		SaveInstanceState,

		/// <summary>The activity is started.</summary>
		Started,

		/// <summary>The activity is stopped.</summary>
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
