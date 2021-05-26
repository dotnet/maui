using System;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IViewHandler handler, IEditor editor) { }
		public static void MapTextColor(IViewHandler handler, IEditor editor) { }
		public static void MapPlaceholder(IViewHandler handler, IEditor editor) { }
		public static void MapPlaceholderColor(IViewHandler handler, IEditor editor) { }
		public static void MapCharacterSpacing(IViewHandler handler, IEditor editor) { }
		public static void MapMaxLength(IViewHandler handler, IEditor editor) { }
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }
		public static void MapFont(IViewHandler handler, IEditor editor) { }
		public static void MapIsReadOnly(IViewHandler handler, IEditor editor) { }
		public static void MapTextColor(EditorHandler handler, IEditor editor) { }
	}
}