namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(IEditorHandler handler, Editor editor)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, editor);
		}
	}
}
