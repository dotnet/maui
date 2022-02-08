#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, TextBox>
	{
		protected override TextBox CreatePlatformView() =>
			new TextBox
			{
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
			};

		protected override void ConnectHandler(TextBox platformView)
		{
			platformView.TextChanged += OnTextChanged;
			platformView.LostFocus += OnLostFocus;
			platformView.Loaded += OnNativeLoaded;
		}

		protected override void DisconnectHandler(TextBox platformView)
		{
			platformView.Loaded -= OnNativeLoaded;
			platformView.TextChanged -= OnTextChanged;
			platformView.LostFocus -= OnLostFocus;
		}

		public static void MapText(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateText(editor);

		public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor);

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapFont(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapBackground(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateBackground(editor);

		public static void MapTextColor(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapCursorPosition(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		public static void MapVerticalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		void OnTextChanged(object sender, TextChangedEventArgs args) =>
			VirtualView?.UpdateText(PlatformView.Text);

		void OnLostFocus(object? sender, RoutedEventArgs e) =>
			VirtualView?.Completed();

		void OnNativeLoaded(object sender, RoutedEventArgs e) =>
			MauiTextBox.InvalidateAttachedProperties(PlatformView);
	}
}