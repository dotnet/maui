namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton, IButtonContentLayout
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

		IImageSource IButton.ImageSource => ImageSource;

		Maui.ButtonContentLayout IButtonContentLayout.ContentLayout =>
			new Maui.ButtonContentLayout((Maui.ButtonContentLayout.ImagePosition)(int)ContentLayout.Position, ContentLayout.Spacing);
	}
}
