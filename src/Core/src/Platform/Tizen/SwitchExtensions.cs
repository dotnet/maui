using ElmSharp;
using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this Switch nativeCheck, ISwitch view)
		{
			nativeCheck.IsToggled = view.IsOn;
		}

		public static void UpdateTrackColor(this Switch nativeCheck, ISwitch view)
		{
			nativeCheck.OnColor = view.TrackColor.ToNative();
		}

		public static void UpdateThumbColor(this Switch nativeCheck, ISwitch view)
		{
			nativeCheck.ThumbColor = view.ThumbColor.ToNative();
		}
	}
}