using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this Switch platformCheck, ISwitch view)
		{
			platformCheck.IsToggled = view.IsOn;
		}

		public static void UpdateTrackColor(this Switch platformCheck, ISwitch view)
		{
			platformCheck.OnColor = view.TrackColor.ToPlatform();
		}

		public static void UpdateThumbColor(this Switch platformCheck, ISwitch view)
		{
			platformCheck.ThumbColor = view.ThumbColor.ToPlatform();
		}
	}
}