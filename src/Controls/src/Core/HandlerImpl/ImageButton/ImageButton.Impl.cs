#nullable disable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/ImageButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.ImageButton']/Docs/*" />
	public partial class ImageButton : IImageButton
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == BorderWidthProperty.PropertyName)
				Handler?.UpdateValue(nameof(IImageButton.StrokeThickness));
			else if (propertyName == BorderColorProperty.PropertyName)
				Handler?.UpdateValue(nameof(IImageButton.StrokeColor));
		}

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
