using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		// IButton does not include the ContentType property, so we map it here to handle Image Positioning

		/// <summary>
		/// The property mapper that maps the abstract properties to the platform-specific methods for further processing.
		/// </summary>
		public static IPropertyMapper<IButton, ButtonHandler> ControlsButtonMapper = new PropertyMapper<Button, ButtonHandler>(ButtonHandler.Mapper)
		{
			[nameof(ContentLayout)] = MapContentLayout,
#if IOS
			[nameof(Padding)] = MapPadding,
#endif
#if WINDOWS
			[nameof(IText.Text)] = MapText,
			[nameof(ImageSource)] = MapImageSource,
#endif
			[nameof(TextTransform)] = MapText,
			[nameof(Text)] = MapText,
			[nameof(Button.LineBreakMode)] = MapLineBreakMode,
		};

		internal new static void RemapForControls()
		{
			ButtonHandler.Mapper = ControlsButtonMapper;
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
