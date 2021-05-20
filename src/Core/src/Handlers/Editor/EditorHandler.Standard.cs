using System;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : WidgetHandler<IEditor, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapTextColor(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapPlaceholder(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapPlaceholderColor(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapCharacterSpacing(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapMaxLength(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }
		public static void MapFont(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapIsReadOnly(IFrameworkElementHandler handler, IEditor editor) { }
		public static void MapTextColor(EditorHandler handler, IEditor editor) { }
	}
}