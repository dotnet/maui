namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(EditorHandler handler, Editor editor)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, editor);
		}
	}
}
