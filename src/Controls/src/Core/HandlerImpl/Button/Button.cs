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

		public static IPropertyMapper<IButton, ButtonHandler> ControlsButtonMapper = new PropertyMapper<Button, ButtonHandler>(ButtonHandler.Mapper)
		{
			[nameof(ContentLayout)] = MapContentLayout,
#if __IOS__
			[nameof(Padding)] = MapPadding,
#endif
#if WINDOWS
			[nameof(IText.Text)] = MapText,
			[nameof(ImageSource)] = MapImageSource,
#endif
			[nameof(TextTransform)] = MapText,
			[nameof(Text)] = MapText,
		};

		internal new static void RemapForControls()
		{
			ButtonHandler.Mapper = ControlsButtonMapper;
		}

		public static void MapContentLayout(ButtonHandler handler, Button button)
		{
			handler.NativeView.UpdateContentLayout(button);
		}
	}
}
