using Android.Content;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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