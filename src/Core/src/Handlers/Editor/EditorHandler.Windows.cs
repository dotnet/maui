#nullable enable
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiTextBox>
	{
		protected override MauiTextBox CreateNativeView() => new MauiTextBox
		{
			AcceptsReturn = true,
			TextWrapping = TextWrapping.Wrap,
			VerticalContentAlignment = VerticalAlignment.Top
		};

		protected override void ConnectHandler(MauiTextBox nativeView)
		{
			nativeView.LostFocus += OnLostFocus;
		}

		protected override void DisconnectHandler(MauiTextBox nativeView)
		{
			nativeView.LostFocus -= OnLostFocus;
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateText(editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdatePlaceholder(editor);
		}

		[MissingMapper]
		public static void MapPlaceholderColor(IViewHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IViewHandler handler, IEditor editor) { }

		public static void MapMaxLength(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateMaxLength(editor);
		}

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }

		public static void MapFont(EditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(editor, fontManager);
		}

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateIsReadOnly(editor);
		}

		public static void MapTextColor(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateTextColor(editor);
		
		void OnLostFocus(object? sender, RoutedEventArgs e)
		{
			VirtualView?.Completed();
		}
	}
}
