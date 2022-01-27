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
		public static void MapIsSpellCheckEnabled(IViewHandler handler, IEditor editor) { }
		public static void MapIsTextPredictionEnabled(IViewHandler handler, IEditor editor) { }
		public static void MapFont(IViewHandler handler, IEditor editor) { }
		public static void MapIsReadOnly(IViewHandler handler, IEditor editor) { }
		public static void MapTextColor(IViewHandler handler, IEditor editor) { }
		public static void MapHorizontalTextAlignment(IViewHandler handler, IEditor editor) { }
		public static void MapVerticalTextAlignment(IViewHandler handler, IEditor editor) { }
		public static void MapKeyboard(IViewHandler handler, IEditor editor) { }
		public static void MapCursorPosition(IViewHandler handler, ITextInput editor) { }
		public static void MapSelectionLength(IViewHandler handler, ITextInput editor) { }
	}
}
