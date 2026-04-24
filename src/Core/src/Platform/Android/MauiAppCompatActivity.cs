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

			// Register with the AndroidX OnBackPressedDispatcher (all API levels 21+).
			// On Android 13+/API 33+, AndroidX automatically bridges this to OnBackInvokedDispatcher,
			// enabling the predictive back gesture without separate registration.
			// See: https://developer.android.com/guide/navigation/custom-back/predictive-back-gesture
			OnBackPressedDispatcher.AddCallback(this, new MauiOnBackPressedCallback(this));
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

		/// <summary>
		/// Callback registered with <see cref="OnBackPressedDispatcher"/> to route back navigation
		/// through the MAUI lifecycle. Replaces the deprecated OnBackPressed() override.
		/// The callback is lifecycle-aware and automatically removed when the activity is destroyed.
		/// </summary>
		sealed class MauiOnBackPressedCallback : OnBackPressedCallback
		{
			readonly MauiAppCompatActivity _activity;

			public MauiOnBackPressedCallback(MauiAppCompatActivity activity) : base(true /* enabled */)
			{
				_activity = activity;
			}

			public override void HandleOnBackPressed()
			{
				if (!_activity.HandleBackNavigation())
				{
					// MAUI did not handle back — temporarily disable this callback so the
					// dispatcher can proceed with default behavior (e.g., finish the activity).
					Enabled = false;
					try
					{
						_activity.OnBackPressedDispatcher.OnBackPressed();
					}
					finally
					{
						Enabled = true;
					}
				}
			}
		}
	}
}