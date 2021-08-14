using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton : IRadioButton, INotifyFontChanging
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		void INotifyFontChanging.FontChanging()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;
		}
	}
}