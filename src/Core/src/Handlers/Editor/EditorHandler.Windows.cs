using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, TextBox>
	{
		protected override TextBox CreateNativeView() => new TextBox();

		[MissingMapper]
		public static void MapText(IViewHandler handler, IEditor editor) { }

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

		[MissingMapper]
		public static void MapKeyboard(EditorHandler handler, IEditor editor) { }
	}
}