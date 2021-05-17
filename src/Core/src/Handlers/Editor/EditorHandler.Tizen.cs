using System;
using Tizen.UIExtensions.ElmSharp;
using EEntry = ElmSharp.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : EViewHandler<IEditor, Entry>
	{
		protected override Entry CreateNativeView()
		{
			return new EditfieldEntry(NativeParent, EditFieldEntryLayout.Styles.MulitLine)
			{
				IsSingleLine = false
			};
		}

		protected override void ConnectHandler(Entry nativeView)
		{
			nativeView.Focused += OnEntryFocused;
			nativeView.Unfocused += OnEntryUnfocused;
			nativeView.Unfocused += OnCompleted;
			nativeView.PrependMarkUpFilter(MaxLengthFilter);
			nativeView.TextChanged += OnTextChanged;
		}

		protected override void DisconnectHandler(Entry nativeView)
		{
			nativeView.BackButtonPressed -= OnCompleted;
			nativeView.Unfocused -= OnEntryUnfocused;
			nativeView.Focused -= OnEntryFocused;
			nativeView.TextChanged -= OnTextChanged;
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateText(editor);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, editor);
		}

		public static void MapTextColor(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateTextColor(editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdatePlaceholderColor(editor);
		}

		public static void MapMaxLength(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateMaxLength(editor);
		}

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateIsReadOnly(editor);
		}

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateIsTextPredictionEnabled(editor);
		}

		public static void MapFont(EditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(editor, fontManager);
		}

		public static void MapFormatting(EditorHandler handler, IEditor editor)
		{
			// Update all of the attributed text formatting properties
			handler.NativeView?.UpdateMaxLength(editor);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) { }

		string? MaxLengthFilter(EEntry entry, string s)
		{
			if (VirtualView == null || NativeView == null)
				return null;

			if (entry.Text.Length < VirtualView.MaxLength)
				return s;

			return null;
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Text = NativeView.Text;
		}

		

		void OnEntryFocused(object sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			// BackButtonPressed is only passed to the object that is at the highest Z-Order, and it does not propagate to lower objects.
			// If you want to make Editor input completed by using BackButtonPressed, you should subscribe BackButtonPressed event only when Editor gets focused.
			NativeView.BackButtonPressed += OnCompleted;
		}

		void OnEntryUnfocused(object sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			// BackButtonPressed is only passed to the object that is at the highest Z-Order, and it does not propagate to lower objects.
			// When the object is unfocesed BackButtonPressed event has to be released to stop using it.
			NativeView.BackButtonPressed -= OnCompleted;
		}

		void OnCompleted(object sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			NativeView.SetFocus(false);
		}
	}
}