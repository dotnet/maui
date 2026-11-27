#nullable disable
using System;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		// IButton does not include the ContentType property, so we map it here to handle Image Positioning
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(ContentLayout), MapContentLayout);
#if IOS
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(Padding), MapPadding);
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(BorderWidth), MapBorderWidth);
#endif
#if ANDROID
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(PlatformConfiguration.AndroidSpecific.Button.RippleColorProperty.PropertyName, MapRippleColor);
			ButtonHandler.Mapper.AppendToMappingForControls<Button, IButtonHandler>(nameof(Background), MapRippleColor);
#endif
#if WINDOWS
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(ImageSource), MapImageSource);
#endif
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(Text), MapText);

			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(TextTransform), MapText);
			ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(Button.LineBreakMode), MapLineBreakMode);
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
