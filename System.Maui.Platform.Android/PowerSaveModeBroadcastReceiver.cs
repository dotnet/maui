using Android.Content;
using System.Maui.Internals;

namespace System.Maui.Platform.Android
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