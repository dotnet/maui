using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.OS;

namespace Xamarin.Essentials
{
    public abstract class WebAuthenticatorCallbackActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            WebAuthenticator.OnResume(Intent);

            Finish();
        }
    }
}
