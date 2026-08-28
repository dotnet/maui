using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Microsoft.Maui.Authentication
{
	[Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, Exported = false)]
	class WebAuthenticatorIntermediateActivity : Activity
	{
		internal const string LaunchedExtra = "launched";
		internal const string ActualIntentExtra = "actual_intent";

		bool launched;
		Intent actualIntent;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var extras = savedInstanceState ?? Intent.Extras;

			if (extras == null)
			{
				return;
			}

			// read the values
			launched = extras.GetBoolean(LaunchedExtra, false);

			if (OperatingSystem.IsAndroidVersionAtLeast(33))
				actualIntent = extras.GetParcelable(ActualIntentExtra, Java.Lang.Class.FromType(typeof(Intent))) as Intent;
			else
				actualIntent = extras.GetParcelable(ActualIntentExtra) as Intent;
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (!launched)
			{
				// if this is the first time, start the authentication flow
				StartActivity(actualIntent);

				launched = true;
			}
			else
			{
				// otherwise, resume the auth flow and finish this activity
				WebAuthenticator.Default.OnResume(Intent!);

				Finish();
			}
		}

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);

			Intent = intent;
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			// save the values
			outState.PutBoolean(LaunchedExtra, launched);
			outState.PutParcelable(ActualIntentExtra, actualIntent);

			base.OnSaveInstanceState(outState);
		}

		public static void StartActivity(Activity activity, Intent intent)
		{
			var intermediateIntent = new Intent(activity, typeof(WebAuthenticatorIntermediateActivity));
			intermediateIntent.PutExtra(ActualIntentExtra, intent);

			activity.StartActivity(intermediateIntent);
		}
	}
}
