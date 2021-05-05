namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}