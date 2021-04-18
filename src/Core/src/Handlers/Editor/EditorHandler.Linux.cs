using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, TextView>
	{
		protected override TextView CreateNativeView()
		{
			return new TextView();
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateText(editor);
		}

		[MissingMapper]
		public static void MapPlaceholder(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapPlaceholderColor(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapMaxLength(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapFont(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapTextColor(EditorHandler handler, IEditor editor) { }
	}
}