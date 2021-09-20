using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton, IText, IImageSourcePart
	{
		public new static void RemapForControls()
		{
			// IButton does not include the ContentType property, so we map it here to handle Image Positioning

			IPropertyMapper<IButton, IButtonHandler> ControlsButtonMapper = new PropertyMapper<Button, IButtonHandler>(ButtonMapper.Mapper)
			{
				[nameof(ContentLayout)] = MapContentLayout,
#if __IOS__
				[nameof(Padding)] = MapPadding,
#endif
			};

			ButtonMapper.Mapper = ControlsButtonMapper;
		}

		public static void MapContentLayout(IButtonHandler handler, Button button)
		{
#if __IOS__
			(handler.NativeView as UIKit.UIButton)?.UpdateContentLayout(button);
#elif  __ANDROID__
			(handler.NativeView as Google.Android.Material.Button.MaterialButton)?.UpdateContentLayout(button);
#endif
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

		IImageSource IImageSourcePart.Source => ImageSource;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) 
		{
			if (!isLoading)
				Handler?.UpdateValue(nameof(ContentLayout));
		}

		bool IImageSourcePart.IsAnimationPlaying => false;

		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}
