using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Services
{
	public class CustomAndroidLifecycleHandler : AndroidApplicationLifetime
	{
		public override void OnCreate(Activity activity, Bundle savedInstanceState) => LogMember();

		public override void OnPostCreate(Activity activity, Bundle savedInstanceState) => LogMember();

		public override void OnDestroy(Activity activity) => LogMember();

		public override void OnPause(Activity activity) => LogMember();

		public override void OnResume(Activity activity) => LogMember();

		public override void OnPostResume(Activity activity) => LogMember();

		public override void OnStart(Activity activity) => LogMember();

		public override void OnStop(Activity activity) => LogMember();

		public override void OnRestart(Activity activity) => LogMember();

		public override void OnSaveInstanceState(Activity activity, Bundle outState) => LogMember();

		public override void OnRestoreInstanceState(Activity activity, Bundle savedInstanceState) => LogMember();

		public override void OnConfigurationChanged(Activity activity, Configuration newConfig) => LogMember();

		public override void OnActivityResult(Activity activity, int requestCode, Result resultCode, Intent data) => LogMember();

		static void LogMember([CallerMemberName] string name = "") =>
			System.Diagnostics.Debug.WriteLine("LIFECYCLE: " + name);
	}
}