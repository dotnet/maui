namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}
	}
}