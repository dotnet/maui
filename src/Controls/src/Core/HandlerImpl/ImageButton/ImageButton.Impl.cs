using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.ImageButton']/Docs/*" />
	public partial class ImageButton : IImageButton
	{
		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
			((IImageController)this)?.SetIsLoading(isLoading);
		}

		bool IImageSourcePart.IsAnimationPlaying => false;

		IImageSource IImageSourcePart.Source => Source;

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

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);
	}
}
