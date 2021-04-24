namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font? _font;

		TextAlignment _horizontalTextAlignment = TextAlignment.Start;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		public TextAlignment HorizontalTextAlignment { get { return _horizontalTextAlignment; } set { _horizontalTextAlignment = value; } }
	}
}