using System;
using Android.OS;
using Android.Views;
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
				RuntimeFeature.IsMaterial3Enabled
				? Resource.Style.Maui_Material3_Theme_NoActionBar
				: Resource.Style.Maui_MainTheme_NoActionBar);

			base.OnCreate(savedInstanceState);
			WindowCompat.SetDecorFitsSystemWindows(Window, false);

			if (IPlatformApplication.Current?.Application is not null)
			{
				this.CreatePlatformWindow(IPlatformApplication.Current.Application, savedInstanceState);
			}

			// Use OnBackPressedCallback (AndroidX) so the system predictive back-to-home
			// animation plays when the app has nothing to handle (IsEnabled = false).
			// IOnBackInvokedCallback (Android 13+ API) was avoided here because registering
			// one always suppresses the back-to-home animation regardless of IsEnabled.
			_mauiOnBackPressedCallback = new MauiOnBackPressedCallback(this);
			OnBackPressedDispatcher.AddCallback(this, _mauiOnBackPressedCallback);
			UpdatePredictiveBackRegistration();
		}

		protected override void OnDestroy()
		{
			_mauiOnBackPressedCallback?.Remove();
			_mauiOnBackPressedCallback = null;
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

		MauiOnBackPressedCallback? _mauiOnBackPressedCallback;

		// Must be called at every navigation state change so that Enabled reflects the current back
		// stack before the predictive back drag preview starts (Android reads Enabled before commit).
		// Call sites: Page.SendAppearing, Shell.SendNavigated, NavigationPage, Window.
		internal void UpdatePredictiveBackRegistration()
		{
			if (_mauiOnBackPressedCallback is null)
				return;

			_mauiOnBackPressedCallback.Enabled = ShouldRegisterPredictiveBackCallback();
		}

		bool ShouldRegisterPredictiveBackCallback()
		{
			var services = IPlatformApplication.Current?.Services;
			if (services is null)
				return false;

			// Iterate with early exit to avoid per-navigation heap allocation from ToArray()/Any().
			bool hasAnyHandler = false;
			bool hasCustomBackHandler = false;
			foreach (var handler in services.GetLifecycleEventDelegates<AndroidLifecycle.OnBackPressed>())
			{
				hasAnyHandler = true;
				if (handler != AppHostBuilderExtensions.DefaultWindowBackHandler)
				{
					hasCustomBackHandler = true;
					break;
				}
			}

			if (!hasAnyHandler)
				return false;

			return hasCustomBackHandler || this.GetWindow() is IBackNavigationState { CanConsumeBackNavigation: true };
		}

		sealed class MauiOnBackPressedCallback : OnBackPressedCallback
		{
			readonly MauiAppCompatActivity _activity;
			public MauiOnBackPressedCallback(MauiAppCompatActivity activity) : base(false)
			{
				_activity = activity;
			}

			public override void HandleOnBackPressed()
			{
				_activity.HandleBackNavigation();
			}
		}
	}
}
