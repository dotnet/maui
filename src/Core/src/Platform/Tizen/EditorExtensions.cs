using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdatePlaceholderColor(this Entry nativeEntry, IEditor editor)
		{
			nativeEntry.PlaceholderColor = editor.PlaceholderColor.ToNative();
		}
	}
}
