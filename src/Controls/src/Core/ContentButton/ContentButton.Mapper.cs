using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class ContentButton
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);

		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
#if ANDROID
			ContentButtonHandler.Mapper.ReplaceMappingForControls<ContentButton, IContentButtonHandler>(
				PlatformConfiguration.AndroidSpecific.ContentButton.RippleColorProperty.PropertyName,
				MapRippleColor);
			ContentButtonHandler.Mapper.AppendToMappingForControls<ContentButton, IContentButtonHandler>(
				nameof(Background),
				(handler, _) => handler.UpdateValue(PlatformConfiguration.AndroidSpecific.ContentButton.RippleColorProperty.PropertyName));
#endif
		}
	}
}
