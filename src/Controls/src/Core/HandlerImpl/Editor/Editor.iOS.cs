namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapAutoSize(EditorHandler handler, Editor editor)
		{
			handler.PlatformView?.UpdateShouldChangeScrollPosition(
				editor.AutoSize == EditorAutoSizeOption.TextChanges);
		}
		
		public static void MapText(EditorHandler handler, Editor editor) 
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, editor);
		}
	}
}
