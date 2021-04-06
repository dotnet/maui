namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton
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

		Font IText.Font => Font;

		Thickness IPadding.Padding => IsSet(PaddingProperty) ? Padding : new Thickness(double.NaN);
	}
}