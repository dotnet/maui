using System;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;
using static Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	// TODO: NET8 issoto - Change the TPlatformView generic type to MauiAppCompatEditText
	// This type adds support to the SelectionChanged event
	public partial class EditorHandler : ViewHandler<IEditor, AppCompatEditText>
	{
		bool _set;

		// TODO: NET8 issoto - Change the return type to MauiAppCompatEditText
		protected override AppCompatEditText CreatePlatformView()
		{
			var editText = new MauiAppCompatEditText(Context)
			{
				ImeOptions = ImeAction.Done,
				Gravity = GravityFlags.Top,
				TextAlignment = Android.Views.TextAlignment.ViewStart,
			};

			editText.SetSingleLine(false);
			editText.SetHorizontallyScrolling(false);

			return editText;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			// TODO: NET8 issoto - Remove the casting once we can set the TPlatformView generic type as MauiAppCompatEditText
			if (!_set && PlatformView is MauiAppCompatEditText editText)
				editText.SelectionChanged += OnSelectionChanged;

			_set = true;
		}

		// TODO: NET8 issoto - Change the platformView type to MauiAppCompatEditText
		protected override void ConnectHandler(AppCompatEditText platformView)
		{
			platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;
			platformView.TextChanged += OnTextChanged;
		}

		// TODO: NET8 issoto - Change the platformView type to MauiAppCompatEditText
		protected override void DisconnectHandler(AppCompatEditText platformView)
		{
			platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
			platformView.TextChanged -= OnTextChanged;

			// TODO: NET8 issoto - Remove the casting once we can set the TPlatformView generic type as MauiAppCompatEditText
			if (_set && platformView is MauiAppCompatEditText editText)
				editText.SelectionChanged -= OnSelectionChanged;

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

		void OnPlatformViewAttachedToWindow(object? sender, ViewAttachedToWindowEventArgs e)
		{
			if (PlatformView.IsAlive() && PlatformView.Enabled)
			{
				// https://issuetracker.google.com/issues/37095917
				PlatformView.Enabled = false;
				PlatformView.Enabled = true;
			}
		}

		void OnTextChanged(object? sender, Android.Text.TextChangedEventArgs e)
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

		public override void PlatformArrange(Rect frame)
		{
			this.PrepareForTextViewArrange(frame);
			base.PlatformArrange(frame);
		}
	}
}