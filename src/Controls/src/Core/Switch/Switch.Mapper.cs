namespace Microsoft.Maui.Controls;

public partial class Switch
{
	internal static new void RemapForControls()
	{
#if ANDROID
			SwitchHandler.Mapper.AppendToMapping<Switch, ISwitchHandler>(nameof(ISwitch.TrackColor), MapTrackColor);
#endif
	}

#if ANDROID
	private static void MapTrackColor(ISwitchHandler handler, Switch view)
	{
		if (view.IsToggled)
		{
			return;
		}

		if (handler.PlatformView is AndroidX.AppCompat.Widget.SwitchCompat aSwitch)
		{
			//https://android.googlesource.com/platform/frameworks/support/+/9d5f84f/v7/appcompat/res/color/abc_tint_switch_track.xml
			var isDarkTheme = Application.Current?.UserAppTheme != ApplicationModel.AppTheme.Unspecified
				? Application.Current?.UserAppTheme == ApplicationModel.AppTheme.Dark
				: Application.Current.PlatformAppTheme == ApplicationModel.AppTheme.Dark;

			var trackMaterial = isDarkTheme ? Resource.Color.foreground_material_dark : Resource.Color.foreground_material_light;
			var trackColor = new Android.Graphics.Color(AndroidX.Core.Content.ContextCompat.GetColor(handler.PlatformView.Context, trackMaterial));

			var alphaChannel = view.IsEnabled ? 0.3f : 0.1f;
			var trackColorWithAlpha = new Android.Graphics.Color(trackColor.R, trackColor.G, trackColor.B, (int)(trackColor.A * alphaChannel));

			aSwitch.TrackDrawable?.SetColorFilter(trackColorWithAlpha, FilterMode.SrcIn);
		}
	}
}
#endif