using System;
using System.Collections.Generic;
using System.Text;

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
	}
}
