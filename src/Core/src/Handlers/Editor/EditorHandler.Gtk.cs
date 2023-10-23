using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.TextView.html 

	public partial class EditorHandler : ViewHandler<IEditor, TextView>
	{

		protected override TextView CreatePlatformView()
		{
			return new() { WrapMode = WrapMode.WordChar };
		}

		protected override void ConnectHandler(TextView nativeView)
		{
			nativeView.Buffer.Changed += OnNativeTextChanged;
		}

		protected override void DisconnectHandler(TextView nativeView)
		{
			nativeView.Buffer.Changed -= OnNativeTextChanged;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (PlatformView is not { } nativeView)
				return Size.Zero;

			var res = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (res.Height != 0 && res.Width != 0)
				return res;

			var minSize = (int)Math.Round(nativeView.GetFontHeigth());

			if (res.Height == 0)
				res.Height = minSize;

			if (res.Width == 0)
				res.Width = minSize;

			return res;
		}

		protected void OnNativeTextChanged(object? sender, EventArgs e)
		{
			if (PlatformView is not { } nativeView || VirtualView is not { } virtualView)
				return;

			if (sender != nativeView.Buffer) return;

			var text = nativeView.Buffer.Text;

			if (virtualView.Text != text)
				virtualView.Text = text;
		}

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateText(editor);
		}

		public static void MapFont(IEditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(editor, fontManager);
		}

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor)
		{
			if (handler.PlatformView is { } nativeView)
				nativeView.Editable = !editor.IsReadOnly;
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateTextColor(editor.TextColor);
		}

		[MissingMapper]
		public static void MapPlaceholder(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor)
		{
			// see: https://docs.gtk.org/gtk3/property.TextTag.letter-spacing.html
		}

		[MissingMapper]
		public static void MapMaxLength(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapKeyboard(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapCursorPosition(IEditorHandler handler, ITextInput editor) { }

		[MissingMapper]
		public static void MapSelectionLength(IEditorHandler handler, ITextInput editor) { }

	}

}