namespace Microsoft.Maui.Controls
{
	public partial class Picker : IPicker
	{
		Font? _font;

		Font IPicker.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}