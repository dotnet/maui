namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize, enableScaling: FontScalingEnabled).WithAttributes(FontAttributes);

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}
	}
}