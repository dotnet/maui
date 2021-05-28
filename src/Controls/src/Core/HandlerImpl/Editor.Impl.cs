namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}
	}
}