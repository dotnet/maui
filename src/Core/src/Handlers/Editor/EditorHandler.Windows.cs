#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, TextBox>
	{
		protected override TextBox CreateNativeView() =>
			new TextBox
			{
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
			};

		protected override void ConnectHandler(TextBox nativeView)
		{
			nativeView.TextChanged += OnTextChanged;
			nativeView.LostFocus += OnLostFocus;
			nativeView.Loaded += OnNativeLoaded;
		}

		protected override void DisconnectHandler(TextBox nativeView)
		{
			nativeView.Loaded -= OnNativeLoaded;
			nativeView.TextChanged -= OnTextChanged;
			nativeView.LostFocus -= OnLostFocus;
		}

		public static void MapText(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateText(editor);

		public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdatePlaceholderColor(editor);

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateMaxLength(editor);

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapFont(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateIsReadOnly(editor);

		public static void MapBackground(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateBackground(editor);

		public static void MapTextColor(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateTextColor(editor);

		public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateHorizontalTextAlignment(editor);

		public static void MapCursorPosition(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateSelectionLength(editor);

		public static void MapVerticalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(EditorHandler handler, IEditor editor) =>
			handler.NativeView?.UpdateKeyboard(editor);

		void OnTextChanged(object sender, TextChangedEventArgs args) =>
			VirtualView?.UpdateText(NativeView.Text);

		void OnLostFocus(object? sender, RoutedEventArgs e) =>
			VirtualView?.Completed();

		void OnNativeLoaded(object sender, RoutedEventArgs e) =>
			MauiTextBox.InvalidateAttachedProperties(NativeView);
	}
}