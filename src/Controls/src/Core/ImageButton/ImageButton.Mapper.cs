namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		static ImageButton() => RemapForControls();

		private new static void RemapForControls()
		{
			VisualElement.RemapIfNeeded();

#if ANDROID
			ImageButtonHandler.Mapper.ReplaceMapping<ImageButton, IImageButtonHandler>(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty.PropertyName, MapRippleColor);
			ImageButtonHandler.Mapper.AppendToMapping<ImageButton, IImageButtonHandler>(nameof(Background), MapRippleColor);
#endif
		}
	}
}
