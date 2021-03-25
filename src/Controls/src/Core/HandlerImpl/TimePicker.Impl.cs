namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font? _font;

		Font ITimePicker.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}