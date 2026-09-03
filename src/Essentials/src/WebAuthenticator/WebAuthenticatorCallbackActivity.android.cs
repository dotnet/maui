using Android.App;
using Android.Content;
using Android.OS;

namespace Microsoft.Maui.Authentication
{
	public abstract class WebAuthenticatorCallbackActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Check how we launched the flow initially
			if (WebAuthenticator.Default.IsAuthenticatingWithCustomTabs())
			{
				// start the intermediate activity again with flags to close the custom tabs
				var intent = new Intent(this, typeof(WebAuthenticatorIntermediateActivity));
				intent.SetData(Intent.Data);
				intent.PutExtra(WebAuthenticatorIntermediateActivity.LaunchedExtra, true);
				intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
				StartActivity(intent);
			}
			else
			{
				// No intermediate activity if we returned from a system browser
				// intent since there's no custom tab instance to clean up
				WebAuthenticator.Default.OnResume(Intent);
			}

			// finish this activity
			Finish();
		}
	}
}
