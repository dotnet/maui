namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font? _font;

		Font IText.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		string IText.Text => this.Time.ToFormattedString(this.Format);
	}
}