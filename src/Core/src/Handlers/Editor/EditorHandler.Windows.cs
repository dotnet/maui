using System;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiTextBox>
	{
		protected override MauiTextBox CreateNativeView() => new MauiTextBox();

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

		public static void MapForeground(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateForeground(editor);
	}
}
