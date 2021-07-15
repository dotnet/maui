namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton
	{
		CornerRadius IButton.CornerRadius =>
			new CornerRadius(CornerRadius);

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

		Font ITextStyle.Font => Font;
	}
}