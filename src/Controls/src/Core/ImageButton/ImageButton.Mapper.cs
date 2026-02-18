namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		static ImageButton()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
RemappingHelper.EnsureBaseTypeRemapped(typeof(ImageButton), typeof(VisualElement));

#if ANDROID
			ImageButtonHandler.Mapper.ReplaceMapping<ImageButton, IImageButtonHandler>(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty.PropertyName, MapRippleColor);
			ImageButtonHandler.Mapper.AppendToMapping<ImageButton, IImageButtonHandler>(nameof(Background), MapRippleColor);
#endif
		}
	}
}
