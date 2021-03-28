using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity
	{
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnActivityResult>(del => del(this, requestCode, resultCode, data));
		}

		public override void OnBackPressed()
		{
			var preventBack = false;

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPressingBack>(del =>
			{
				preventBack = del(this) || preventBack;
			});

			if (!preventBack)
				base.OnBackPressed();

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnBackPressed>(del => del(this));
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnConfigurationChanged>(del => del(this, newConfig));
		}

		protected override void OnPostCreate(Bundle? savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPostCreate>(del => del(this, savedInstanceState));
		}

		protected override void OnPostResume()
		{
			base.OnPostResume();

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPostResume>(del => del(this));
		}

		protected override void OnRestart()
		{
			base.OnRestart();

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnRestart>(del => del(this));
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnRestoreInstanceState>(del => del(this, savedInstanceState));
		}
	}
}
