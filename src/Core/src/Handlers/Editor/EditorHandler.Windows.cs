using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, TextBox>
	{
		protected override TextBox CreateNativeView() => new TextBox();

		public static void MapText(IViewHandler handler, IEditor editor) { }
		public static void MapPlaceholder(IViewHandler handler, IEditor editor) { }
		public static void MapPlaceholderColor(IViewHandler handler, IEditor editor) { }
		public static void MapCharacterSpacing(IViewHandler handler, IEditor editor) { }
		public static void MapMaxLength(IViewHandler handler, IEditor editor) { }
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }
		public static void MapFont(IViewHandler handler, IEditor editor) { }
		public static void MapIsReadOnly(IViewHandler handler, IEditor editor) { }
	}
}