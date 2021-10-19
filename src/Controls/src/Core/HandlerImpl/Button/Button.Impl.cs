namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton, IText
	{
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