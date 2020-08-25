using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Xamarin.Forms;

namespace System.Maui.Sample.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
			// Just a stand in to get everything started up for now
			MvvmMauiApplication
				.Current
				.InitWindow(this);

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
			
			Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

			var contentLayout = FindViewById<ViewGroup>(Resource.Id.contentLayout);

			SetSupportActionBar(toolbar);

			var entry = new Entry().ToNative(this);
			contentLayout.AddView(entry);
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] global::Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
