namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		internal new static void RemapForControls()
		{
#if ANDROID
			ImageButtonHandler.Mapper.ReplaceMapping<ImageButton, IImageButtonHandler>(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty.PropertyName, MapRippleColor);
			ImageButtonHandler.Mapper.AppendToMapping<ImageButton, IImageButtonHandler>(nameof(Background), MapRippleColor);
#endif
		}
	}
}
