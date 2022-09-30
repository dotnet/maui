using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Button.xml" path="Type[@FullName='Microsoft.Maui.Controls.Button']/Docs/*" />
	public partial class Button
	{
		// IButton does not include the ContentType property, so we map it here to handle Image Positioning

		/// <include file="../../../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='ControlsButtonMapper']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='MapContentLayout']/Docs/*" />
		public static void MapContentLayout(IButtonHandler handler, Button button)
		{
			handler.PlatformView.UpdateContentLayout(button);
		}

		public static void MapContentLayout(ButtonHandler handler, Button button) =>
			MapContentLayout((IButtonHandler)handler, button);
	}
}
