using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Editor : IEditor
	{
		Font? _font;

		public Paint Foreground { get; set; }

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}
	}
}