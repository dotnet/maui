using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class ContentButton
	{
		internal static new void RemapForControls()
		{
#if ANDROID
			ContentButtonHandler.Mapper.ReplaceMapping<ContentButton, IContentButtonHandler>(
				PlatformConfiguration.AndroidSpecific.ContentButton.RippleColorProperty.PropertyName,
				MapRippleColor);
			ContentButtonHandler.Mapper.AppendToMapping<ContentButton, IContentButtonHandler>(
				nameof(Background),
				(handler, _) => handler.UpdateValue(PlatformConfiguration.AndroidSpecific.ContentButton.RippleColorProperty.PropertyName));
#endif
		}
	}
}
