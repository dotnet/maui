namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		internal new static void RemapForControls()
		{
#if ANDROID
			ImageButtonHandler.Mapper.ReplaceMappingForControls<ImageButton, IImageButtonHandler>(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty.PropertyName, MapRippleColor);
			ImageButtonHandler.Mapper.AppendToMappingForControls<ImageButton, IImageButtonHandler>(nameof(Background), MapRippleColor);
#endif
		}
	}
}
