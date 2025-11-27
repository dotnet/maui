using System;
using Android.OS;
using Android.Views;
using Android.Window;
using AndroidX.Activity;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content.Resources;
using AndroidX.Core.View;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			Microsoft.Maui.PlatformMauiAppCompatActivity.OnCreate(
				this,
				savedInstanceState,
				AllowFragmentRestore,
				Resource.Attribute.maui_splash,
				Resource.Style.Maui_MainTheme_NoActionBar);

			base.OnCreate(savedInstanceState);
			WindowCompat.SetDecorFitsSystemWindows(Window, false);

			if (IPlatformApplication.Current?.Application is not null)
			{
				this.CreatePlatformWindow(IPlatformApplication.Current.Application, savedInstanceState);
			}

			// Register predictive back callback (Android 13+/API 33+) if available.
			// This integrates MAUI lifecycle OnBackPressed events with the system back gesture animation.
			// Guidance: route custom back handling through AndroidX OnBackPressedDispatcher so
			// predictive back works correctly:
			// https://developer.android.com/guide/navigation/custom-back/predictive-back-gesture#update-custom
			if (OperatingSystem.IsAndroidVersionAtLeast(33) && _predictiveBackCallback is null)
			{
				_predictiveBackCallback = new PredictiveBackCallback(this);
				// Priority 0 = PRIORITY_DEFAULT: callback invoked only when no higher-priority callback handles the event
				OnBackInvokedDispatcher?.RegisterOnBackInvokedCallback(0, _predictiveBackCallback);
			}
		}

		protected override void OnDestroy()
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(33) && _predictiveBackCallback is not null)
			{
				OnBackInvokedDispatcher?.UnregisterOnBackInvokedCallback(_predictiveBackCallback);
				_predictiveBackCallback.Dispose();
				_predictiveBackCallback = null;
			}
			base.OnDestroy();
		}

		public override bool DispatchTouchEvent(MotionEvent? e)
		{
			// For current purposes this needs to get called before we propagate
			// this message out. In Controls this dispatch call will unfocus the 
			// current focused element which is important for timing if we should
			// hide/show the softkeyboard.
			// If you move this to after the xplat call then the keyboard will show up
			// then close
			bool handled = base.DispatchTouchEvent(e);

			bool implHandled =
				(this.GetWindow() as IPlatformEventsListener)?.DispatchTouchEvent(e) == true;

			return handled || implHandled;
		}

		PredictiveBackCallback? _predictiveBackCallback;

		sealed class PredictiveBackCallback : Java.Lang.Object, IOnBackInvokedCallback
		{
			readonly MauiAppCompatActivity _activity;
			public PredictiveBackCallback(MauiAppCompatActivity activity)
			{
				_activity = activity;
			}

			public void OnBackInvoked()
			{
				// Reuse unified handling (will invoke lifecycle events and conditionally propagate).
				_activity.HandleBackNavigation();
			}
		}
	}
}