using Android.Content;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	[BroadcastReceiver(Enabled = true, Exported = false)]
	public class PowerSaveModeBroadcastReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			CheckAnimationEnabledStatus();
		}

		public void CheckAnimationEnabledStatus()
		{
			((AndroidTicker)Ticker.Default).CheckAnimationEnabledStatus();
		}
	}
}