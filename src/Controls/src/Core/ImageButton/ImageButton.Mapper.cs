#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		#pragma warning disable RS0016
		// IImageButton does not include the ContentType property, so we map it here to handle Image Positioning

		/// <summary>
		/// The property mapper that maps the abstract properties to the platform-specific methods for further processing.
		/// </summary>
		[Obsolete("Use ImageButtonHandler.Mapper instead.")]
		public static IPropertyMapper<IImageButton, ImageButtonHandler> ControlsImageButtonMapper = new ControlsMapper<ImageButton, ImageButtonHandler>(ImageButtonHandler.Mapper);
		internal new static void RemapForControls()
		{
#if ANDROID
			ImageButtonHandler.Mapper.ReplaceMapping<ImageButton, IImageButtonHandler>(nameof(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty), MapRippleColor);
#endif
		}
	}
}
