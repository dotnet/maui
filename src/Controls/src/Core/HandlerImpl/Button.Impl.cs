using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton
	{
		Font? _font;

		public IBrush Foreground { get; set; }

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

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