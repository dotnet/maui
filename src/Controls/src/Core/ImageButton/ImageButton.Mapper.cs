namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		internal new static void RemapForControls()
		{
#if ANDROID
			ImageButtonHandler.Mapper.ReplaceMapping<ImageButton, IImageButtonHandler>(nameof(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty), MapRippleColor);
#endif
		}
	}
}
