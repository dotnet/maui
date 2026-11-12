#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		// IButton does not include the ContentType property, so we map it here to handle Image Positioning

		internal new static void RemapForControls()
		{
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(ContentLayout), MapContentLayout);
#if IOS
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(Padding), MapPadding);
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(BorderWidth), MapBorderWidth);
#endif
#if ANDROID
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(PlatformConfiguration.AndroidSpecific.Button.RippleColorProperty.PropertyName, MapRippleColor);
			ButtonHandler.Mapper.AppendToMapping<Button, IButtonHandler>(nameof(Background), MapRippleColor);
#endif
#if WINDOWS
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(ImageSource), MapImageSource);
#endif
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(Text), MapText);

			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(TextTransform), MapText);
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(Button.LineBreakMode), MapLineBreakMode);
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(HorizontalTextAlignment), MapTextAlignment);
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(VerticalTextAlignment), MapTextAlignment);
		}

		/// <summary>Maps explicitly set text alignment to the Android, iOS, and Windows button implementations.</summary>
		/// <param name="handler">The handler associated with the button.</param>
		/// <param name="button">The button whose alignment is being updated.</param>
		public static void MapTextAlignment(IButtonHandler handler, Button button)
		{
#if ANDROID || IOS || MACCATALYST || WINDOWS
			if (handler is ButtonHandler buttonHandler)
			{
#if WINDOWS
				handler.UpdateValue(nameof(ContentLayout));
#endif
				var hasText = !string.IsNullOrEmpty(button.Text);
				buttonHandler.UpdateTextAlignment(
					hasText && button.IsSetExplicitly(HorizontalTextAlignmentProperty) ? button.HorizontalTextAlignment : null,
					hasText && button.IsSetExplicitly(VerticalTextAlignmentProperty) ? button.VerticalTextAlignment : null);
			}
#endif
		}

		/// <summary>
		/// Maps the abstract <see cref="ContentLayout"/> property to the platform implementation.
		/// </summary>
		/// <param name="handler">The handler associated to this control.</param>
		/// <param name="button">The abstract control that is being mapped.</param>
		public static void MapContentLayout(IButtonHandler handler, Button button)
		{
			handler.PlatformView.UpdateContentLayout(button);
		}

		public static void MapContentLayout(ButtonHandler handler, Button button) =>
			MapContentLayout((IButtonHandler)handler, button);
	}
}
