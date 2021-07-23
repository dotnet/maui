namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}
	}
}