using Android.App;
using Android.Content;
using Android.OS;

namespace Xamarin.Essentials
{
    public abstract class WebAuthenticatorCallbackActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // start the intermediate activity again with flags to close the custom tabs
            var intent = new Intent(this, typeof(WebAuthenticatorIntermediateActivity));
            intent.SetData(Intent.Data);
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);

            // finish this activity
            Finish();
        }
    }
}
