using Android.App;
using Android.OS;

namespace Microsoft.Maui.Authentication
{
	public abstract class WebAuthenticatorCallbackActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			WebAuthenticatorIntermediateActivity.StartCallback(this, Intent?.Data);
			Finish();
		}
	}
}
