namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton, ITextButton, IImageButton
	{
		bool _wasImageLoading;

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

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
			if (!isLoading && _wasImageLoading)
				Handler?.UpdateValue(nameof(ContentLayout));

			_wasImageLoading = isLoading;
		}

		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

		Aspect IImage.Aspect => Aspect.Fill;

		bool IImage.IsOpaque => true;

		IImageSource IImageSourcePart.Source => ImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;
	}
}