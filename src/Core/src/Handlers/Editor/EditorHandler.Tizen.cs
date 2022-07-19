using System;
using Tizen.UIExtensions.ElmSharp;
using EEntry = ElmSharp.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, Entry>
	{
		protected override Entry CreatePlatformView()
		{
			return new EditfieldEntry(PlatformParent, EditFieldEntryLayout.Styles.MulitLine)
			{
				IsSingleLine = false
			};
		}

		protected override void ConnectHandler(Entry platformView)
		{
			platformView.Focused += OnEntryFocused;
			platformView.Unfocused += OnEntryUnfocused;
			platformView.Unfocused += OnCompleted;
			platformView.PrependMarkUpFilter(MaxLengthFilter);
			platformView.TextChanged += OnTextChanged;
		}

		protected override void DisconnectHandler(Entry platformView)
		{
			platformView.BackButtonPressed -= OnCompleted;
			platformView.Unfocused -= OnEntryUnfocused;
			platformView.Focused -= OnEntryFocused;
			platformView.TextChanged -= OnTextChanged;
		}

		public static void MapBackground(IEditorHandler handler, IEditor editor)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(editor);
		}

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateText(editor);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, editor);
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateTextColor(editor);
		}

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdatePlaceholderColor(editor);
		}

		public static void MapMaxLength(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateMaxLength(editor);
		}

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateIsReadOnly(editor);
		}

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);
		}

		public static void MapFont(IEditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(editor, fontManager);
		}

		public static void MapFormatting(IEditorHandler handler, IEditor editor)
		{
			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateMaxLength(editor);
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
		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) { }

		string? MaxLengthFilter(EEntry entry, string s)
		{
			if (VirtualView == null || PlatformView == null)
				return null;

			if (entry.Text.Length < VirtualView.MaxLength)
				return s;

			return null;
		}

		void OnTextChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Text = PlatformView.Text;
		}



		void OnEntryFocused(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			// BackButtonPressed is only passed to the object that is at the highest Z-Order, and it does not propagate to lower objects.
			// If you want to make Editor input completed by using BackButtonPressed, you should subscribe BackButtonPressed event only when Editor gets focused.
			PlatformView.BackButtonPressed += OnCompleted;
		}

		void OnEntryUnfocused(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			// BackButtonPressed is only passed to the object that is at the highest Z-Order, and it does not propagate to lower objects.
			// When the object is unfocesed BackButtonPressed event has to be released to stop using it.
			PlatformView.BackButtonPressed -= OnCompleted;
		}

		void OnCompleted(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			PlatformView.SetFocus(false);
		}
	}
}