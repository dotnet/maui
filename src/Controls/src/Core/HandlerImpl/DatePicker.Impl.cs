namespace Microsoft.Maui.Controls
{
	public partial class DatePicker : IDatePicker
	{
		Font? _font;

		Font IText.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		string IText.Text => this.Date.ToFormattedString(this.Format);
	}
}