#nullable enable
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Xunit;
using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Category("WebAuthenticator")]
public class WebAuthenticatorIntermediateActivity_Tests
{
	[Fact]
	public async Task StartingWithoutExtrasFinishesTheActivity()
	{
		var hostActivity = MauiPlatform.CurrentActivity
			?? throw new InvalidOperationException("The device-test host activity is unavailable.");
		var application = hostActivity.Application
			?? throw new InvalidOperationException("The Android application is unavailable.");

		var callbacks = new IntermediateActivityCallbacks();
		application.RegisterActivityLifecycleCallbacks(callbacks);

		// Android calls the activity's lifecycle methods, so an exception thrown there would crash the app instead of failing the test
		Exception? unhandledException = null;
		AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;

		try
		{
			// Without extras the activity has no authentication intent to launch
			await MainThread.InvokeOnMainThreadAsync(() =>
				hostActivity.StartActivity(new Intent(hostActivity, typeof(WebAuthenticatorIntermediateActivity))));

			await Task.WhenAny(callbacks.Destroyed.Task, Task.Delay(TimeSpan.FromSeconds(15)));
		}
		finally
		{
			AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledException;
			application.UnregisterActivityLifecycleCallbacks(callbacks);

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (callbacks.Activity is { IsDestroyed: false, IsFinishing: false } activity)
					activity.Finish();
			});
		}

		Assert.Null(unhandledException);
		Assert.True(callbacks.Destroyed.Task.IsCompleted, "The activity should finish itself");

		void OnUnhandledException(object? sender, RaiseThrowableEventArgs e)
		{
			unhandledException ??= e.Exception;
			e.Handled = true;
		}
	}

	class IntermediateActivityCallbacks : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		public Activity? Activity { get; private set; }

		public TaskCompletionSource Destroyed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
		{
			if (activity is WebAuthenticatorIntermediateActivity)
				Activity = activity;
		}

		public void OnActivityDestroyed(Activity activity)
		{
			if (activity is WebAuthenticatorIntermediateActivity)
				Destroyed.TrySetResult();
		}

		public void OnActivityPaused(Activity activity)
		{
		}

		public void OnActivityResumed(Activity activity)
		{
		}

		public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
		{
		}

		public void OnActivityStarted(Activity activity)
		{
		}

		public void OnActivityStopped(Activity activity)
		{
		}
	}
}
