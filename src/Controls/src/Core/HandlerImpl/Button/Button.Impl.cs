using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton
	{
		public new static void RemapForControls()
		{
			// IButton does not include the ContentType property, so we map it here to handle Image Positioning

			IPropertyMapper<IButton, ButtonHandler> ControlsButtonMapper = new PropertyMapper<Button, ButtonHandler>(ButtonHandler.ButtonMapper)
			{
				[nameof(ContentLayout)] = MapContentLayout,
#if __IOS__
				[nameof(Padding)] = MapPadding,
#endif
			};

			ButtonHandler.ButtonMapper = ControlsButtonMapper;
		}

		public static void MapContentLayout(ButtonHandler handler, Button button)
		{
			handler.NativeView.UpdateContentLayout(button);
		}

		void IButton.Clicked()
		{
			(this as IButtonController).SendClicked();
		}

		void IButton.Pressed()
		{
			(this as IButtonController).SendPressed();
		}

		void IButton.Released()
		{
			(this as IButtonController).SendReleased();
		}

		void IButton.ImageSourceLoaded()
		{
			Handler?.UpdateValue(nameof(ContentLayout));
		}

		IImageSource IButton.ImageSource => ImageSource;

		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}
