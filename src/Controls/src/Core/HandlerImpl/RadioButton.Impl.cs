namespace Microsoft.Maui.Controls
{
	public partial class RadioButton : IRadioButton
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();
	}
}