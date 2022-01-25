using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class ImageButton : IImageButton
	{
		void IImageSourcePart.UpdateIsLoading(bool isLoading) { }

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
