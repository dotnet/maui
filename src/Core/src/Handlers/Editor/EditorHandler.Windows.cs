using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, TextBox>
	{
		protected override TextBox CreateNativeView() => new TextBox();

		public static void MapText(IViewHandler handler, IEditor editor) { }

		public static void MapCharacterSpacing(IViewHandler handler, IEditor editor) { }

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }
	}
}