using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class EditorExtensions
	{
		public static void UpdatePlaceholderColor(this Entry platformEntry, IEditor editor)
		{
			platformEntry.PlaceholderColor = editor.PlaceholderColor.ToPlatform();
		}
	}
}
