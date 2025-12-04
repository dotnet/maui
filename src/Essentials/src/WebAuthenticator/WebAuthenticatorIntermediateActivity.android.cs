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
#pragma warning disable 618 // TODO: one day use the API 33+ version: https://developer.android.com/reference/android/os/Bundle#getParcelable(java.lang.String,%20java.lang.Class%3CT%3E)
#pragma warning disable CA1422 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
			actualIntent = extras.GetParcelable(ActualIntentExtra) as Intent;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore 618
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
