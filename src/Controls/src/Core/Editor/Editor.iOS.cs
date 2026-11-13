#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		// TODO: Make this public in .NET 11
		internal static void MapAutoSize(IEditorHandler handler, Editor editor)
		{
			if (handler.PlatformView is Microsoft.Maui.Platform.MauiTextView textView)
			{
				textView.AllowAutoGrowth = editor.AutoSize == EditorAutoSizeOption.TextChanges;
			}
		}

		public static void MapText(EditorHandler handler, Editor editor) =>
			MapText((IEditorHandler)handler, editor);

		public static void MapText(IEditorHandler handler, Editor editor)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, editor);

			// Any text changes in the editor field require recalculating the CharacterSpacing by regenerating the attributed string to properly apply the spacing and override the current text formatting.
			handler?.UpdateValue(nameof(CharacterSpacing));
		}
	}
}
