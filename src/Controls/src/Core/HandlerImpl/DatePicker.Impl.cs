namespace Microsoft.Maui.Controls
{
	public partial class DatePicker : IDatePicker
	{
		Font? _font;

		Font IDatePicker.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}