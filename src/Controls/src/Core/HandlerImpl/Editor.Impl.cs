namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font? _font;

		Font IText.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}