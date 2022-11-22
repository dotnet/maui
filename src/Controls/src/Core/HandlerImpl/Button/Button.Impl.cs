using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton, ITextButton, IImageButton
	{
		bool _wasImageLoading;

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == BorderColorProperty.PropertyName)
				Handler?.UpdateValue(nameof(IButtonStroke.StrokeColor));
			else if (propertyName == BorderWidthProperty.PropertyName)
				Handler?.UpdateValue(nameof(IButtonStroke.StrokeThickness));
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

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
			if (!isLoading && _wasImageLoading)
				Handler?.UpdateValue(nameof(ContentLayout));

			_wasImageLoading = isLoading;
		}

		Font ITextStyle.Font => this.ToFont();

		Aspect IImage.Aspect => Aspect.Fill;

		bool IImage.IsOpaque => true;

		IImageSource IImageSourcePart.Source => ImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);
	}
}