namespace Microsoft.Maui.Controls
{
	public partial class SearchBar : ISearchBar
	{
		Font? _font;

		Font IText.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}