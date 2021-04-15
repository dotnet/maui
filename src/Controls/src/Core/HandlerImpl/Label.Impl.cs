namespace Microsoft.Maui.Controls
{
	public partial class Label : ILabel
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}