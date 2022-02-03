using Android.Content.Res;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using static Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, AppCompatEditText>
	{
		ColorStateList? _defaultPlaceholderColors;

		protected override AppCompatEditText CreatePlatformView()
		{
			var editText = new AppCompatEditText(Context)
			{
				ImeOptions = ImeAction.Done,
				Gravity = GravityFlags.Top,
				TextAlignment = Android.Views.TextAlignment.ViewStart,
			};

			editText.SetSingleLine(false);
			editText.SetHorizontallyScrolling(false);

			_defaultPlaceholderColors = editText.HintTextColors;

			return editText;
		}

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			nativeView.TextChanged += OnTextChanged;
			nativeView.FocusChange += OnFocusedChange;
		}

		protected override void DisconnectHandler(AppCompatEditText nativeView)
		{
			nativeView.TextChanged -= OnTextChanged;
			nativeView.FocusChange -= OnFocusedChange;
		}

		public static void MapBackground(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateBackground(editor);

		public static void MapText(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateText(editor);

		public static void MapTextColor(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor, handler._defaultPlaceholderColors);

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapFont(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapVerticalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		public static void MapCursorPosition(EditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(EditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		void OnTextChanged(object? sender, Android.Text.TextChangedEventArgs e) =>
			VirtualView?.UpdateText(e);

		void OnFocusedChange(object? sender, FocusChangeEventArgs e)
		{
			if (!e.HasFocus)
				VirtualView?.Completed();
		}
	}
}
