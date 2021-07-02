using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this Check nativeCheck, ISwitch view)
		{
			nativeCheck.IsChecked = view.IsOn;
		}

		public static void UpdateTrackColor(this Check nativeCheck, ISwitch view)
		{
			if (view.ThumbColor != null)
			{
				nativeCheck.Color = view.TrackColor.ToNativeEFL();
			}
		}

		public static void UpdateThumbColor(this Check nativeCheck, ISwitch view)
		{
			if (view.ThumbColor == null)
			{
				nativeCheck.DeleteOnColors();
			}
			else
			{
				nativeCheck.SetOnColors(view.ThumbColor.ToNativeEFL());
			}
		}
	}
}