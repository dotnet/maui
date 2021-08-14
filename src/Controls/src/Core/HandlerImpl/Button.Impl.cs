using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class Button : IButton, INotifyFontChanging
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

		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		void INotifyFontChanging.FontChanging()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;
		}
	}
}