using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, NView>
	{
		// TODO Need to implement
		protected override NView CreatePlatformView() => new NView()
		{
			BackgroundColor = Tizen.NUI.Color.Red
		};


		public static void MapBackground(IEditorHandler handler, IEditor editor)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(editor);
		}

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapMaxLength(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapFont(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapFormatting(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapKeyboard(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapKeyboard(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateKeyboard(editor);
		}

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);
		}

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);
		}

		public static void MapCursorPosition(IEditorHandler handler, ITextInput editor)
		{
			handler.PlatformView?.UpdateSelectionLength(editor);
		}

		public static void MapSelectionLength(IEditorHandler handler, ITextInput editor)
		{
			handler.PlatformView?.UpdateSelectionLength(editor);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) { }
	}
}