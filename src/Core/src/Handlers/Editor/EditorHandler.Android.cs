using System;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Microsoft.Maui.Graphics;
using static Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiAppCompatEditText>
	{
		bool _set;

		protected override MauiAppCompatEditText CreatePlatformView()
		{
			var editText = new MauiAppCompatEditText(Context)
			{
				ImeOptions = ImeAction.Done,
				TextAlignment = global::Android.Views.TextAlignment.ViewStart,
				Gravity = GravityFlags.Top,
			};

			editText.SetSingleLine(false);
			editText.SetHorizontallyScrolling(false);

			return editText;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (!_set)
				PlatformView.SelectionChanged += OnSelectionChanged;

			_set = true;
		}

		protected override void ConnectHandler(MauiAppCompatEditText platformView)
		{
			platformView.TextChanged += OnTextChanged;
			platformView.FocusChange += OnFocusChange;
		}

		protected override void DisconnectHandler(MauiAppCompatEditText platformView)
		{
			platformView.TextChanged -= OnTextChanged;
			platformView.FocusChange -= OnFocusChange;

			if (_set)
				platformView.SelectionChanged -= OnSelectionChanged;

			_set = false;
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
				handler.PlatformView?.UpdatePlaceholderColor(editor);
		}

		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsSpellCheckEnabled(editor);

		public static void MapFont(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(IEditorHandler handler, IEditor editor)
		{
			handler.UpdateValue(nameof(IEditor.Text));

			handler.PlatformView?.UpdateKeyboard(editor);
		}

		public static void MapCursorPosition(IEditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(IEditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		static void MapFocus(IEditorHandler handler, IEditor editor, object? args)
		{
			if (args is FocusRequest request)
				handler.PlatformView.Focus(request);
		}

		void OnTextChanged(object? sender, TextChangedEventArgs e)
		{
			// Let the mapping know that the update is coming from changes to the platform control
			DataFlowDirection = DataFlowDirection.FromPlatform;
			VirtualView?.UpdateText(e);

			// Reset to the default direction
			DataFlowDirection = DataFlowDirection.ToPlatform;
		}

		private void OnSelectionChanged(object? sender, EventArgs e)
		{
			var cursorPosition = PlatformView.GetCursorPosition();
			var selectedTextLength = PlatformView.GetSelectedTextLength();

			if (VirtualView.CursorPosition != cursorPosition)
				VirtualView.CursorPosition = cursorPosition;

			if (VirtualView.SelectionLength != selectedTextLength)
				VirtualView.SelectionLength = selectedTextLength;
		}

		private void OnFocusChange(object? sender, FocusChangeEventArgs e)
		{
			if (!e.HasFocus)
			{
				VirtualView?.Completed();
			}
		}

		public override void PlatformArrange(Rect frame)
		{
			this.PrepareForTextViewArrange(frame);
			base.PlatformArrange(frame);
		}
	}
}