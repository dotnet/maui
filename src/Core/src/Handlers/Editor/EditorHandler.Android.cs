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

		protected override void ConnectHandler(AppCompatEditText platformView)
		{
			platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;
			platformView.TextChanged += OnTextChanged;
			platformView.FocusChange += OnFocusedChange;
		}

		protected override void DisconnectHandler(AppCompatEditText platformView)
		{
			platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
			platformView.TextChanged -= OnTextChanged;
			platformView.FocusChange -= OnFocusedChange;
		}

		public static void MapBackground(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateBackground(editor);

		public static void MapText(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateText(editor);

		public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor)
		{
			if (handler is EditorHandler platformHandler)
				handler.PlatformView?.UpdatePlaceholderColor(editor, platformHandler._defaultPlaceholderColors);
		}

		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapFont(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		public static void MapCursorPosition(IEditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(IEditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		void OnPlatformViewAttachedToWindow(object? sender, ViewAttachedToWindowEventArgs e)
		{
			if (PlatformView.IsAlive() && PlatformView.Enabled)
			{
				// https://issuetracker.google.com/issues/37095917
				PlatformView.Enabled = false;
				PlatformView.Enabled = true;
			}
		}

		void OnTextChanged(object? sender, Android.Text.TextChangedEventArgs e) =>
			VirtualView?.UpdateText(e);

		void OnFocusedChange(object? sender, FocusChangeEventArgs e)
		{
			if (!e.HasFocus)
				VirtualView?.Completed();
		}
	}
}
