using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Devices;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity
	{
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnActivityResult>(del => del(this, requestCode, resultCode, data));
		}

		// TODO: Investigate whether the new AndroidX way is actually useful:
		//       https://developer.android.com/reference/android/app/Activity#onBackPressed()
		[Obsolete]
#pragma warning disable 809
		public override void OnBackPressed()
#pragma warning restore 809
		{
			HandleBackNavigation();
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnConfigurationChanged>(del => del(this, newConfig));
		}

		protected override void OnNewIntent(Intent? intent)
		{
			base.OnNewIntent(intent);

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnNewIntent>(del => del(this, intent));
		}

		protected override void OnPostCreate(Bundle? savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPostCreate>(del => del(this, savedInstanceState));
		}

		protected override void OnPostResume()
		{
			base.OnPostResume();

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPostResume>(del => del(this));
		}

		protected override void OnRestart()
		{
			base.OnRestart();

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnRestart>(del => del(this));
		}

		[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnRequestPermissionsResult>(del => del(this, requestCode, permissions, grantResults));

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);

			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnRestoreInstanceState>(del => del(this, savedInstanceState));
		}

		public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyDown>(del =>
			{
				handled = del(this, keyCode, e) || handled;
			});

			return handled || base.OnKeyDown(keyCode, e);
		}

		public override bool OnKeyLongPress(Keycode keyCode, KeyEvent? e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyLongPress>(del =>
			{
				handled = del(this, keyCode, e) || handled;
			});

			return handled || base.OnKeyLongPress(keyCode, e);
		}

		public override bool OnKeyMultiple(Keycode keyCode, int repeatCount, KeyEvent? e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyMultiple>(del =>
			{
				handled = del(this, keyCode, repeatCount, e) || handled;
			});

			return handled || base.OnKeyMultiple(keyCode, repeatCount, e);
		}

		public override bool OnKeyShortcut(Keycode keyCode, KeyEvent? e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyShortcut>(del =>
			{
				handled = del(this, keyCode, e) || handled;
			});

			return handled || base.OnKeyShortcut(keyCode, e);
		}

		public override bool OnKeyUp(Keycode keyCode, KeyEvent? e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyUp>(del =>
			{
				handled = del(this, keyCode, e) || handled;
			});

			return handled || base.OnKeyUp(keyCode, e);
		}

		/// <summary>
		/// Central handler used by both legacy <see cref="OnBackPressed"/> and the Android 13+ predictive back gesture callback.
		/// Implements lifecycle event invocation and default back stack propagation unless explicitly prevented.
		/// </summary>
		void HandleBackNavigation()
		{
			var preventBackPropagation = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnBackPressed>(del =>
			{
				preventBackPropagation = del(this) || preventBackPropagation;
			});

			if (!preventBackPropagation)
				base.OnBackPressed();
		}
	}
}