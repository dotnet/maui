using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Button.xml" path="Type[@FullName='Microsoft.Maui.Controls.Button']/Docs" />
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

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);
	}
}