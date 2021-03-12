using Android.App;
using Android.OS;
using Microsoft.Maui;

namespace Maui.Controls.Sample
{
	public class CustomAndroidLifecycleHandler : AndroidLifecycleHandler
	{
		public override void OnCreate(Activity activity, Bundle savedInstanceState)
		{
			base.OnCreate(activity, savedInstanceState);

			System.Diagnostics.Debug.WriteLine("CustomAndroidLifecycleHandler OnCreate");
		}
	}
}