using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{

	// https://developer.gnome.org/gtk3/stable/GtkTextView.html 
	
	public partial class EditorHandler : ViewHandler<IEditor, TextView>
	{

		protected override TextView CreateNativeView()
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
			var res =  base.GetDesiredSize(widthConstraint, heightConstraint);

			if (res.Height == 0 && 	NativeView is { } nativeView)
			{
				res.Height = (int)Math.Round(nativeView.GetFontHeigth());
			}

			return res;
		}

		protected void OnNativeTextChanged(object? sender, EventArgs e)
		{
			if (NativeView is not { } nativeView || VirtualView is not { } virtualView) 
				return;
			
			if (sender != nativeView.Buffer) return;

			var text = nativeView.Buffer.Text;

			if (virtualView.Text != text) 
				virtualView.Text = text;
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateText(editor);
		}

		public static void MapFont(EditorHandler handler, IEditor editor)
		{
			handler.MapFont(editor);
		}

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor)
		{
			if (handler.NativeView is { } nativeView)
				nativeView.Editable = !editor.IsReadOnly;
		}

		public static void MapTextColor(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateTextColor(editor.TextColor);
		}

		[MissingMapper]
		public static void MapPlaceholder(EditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapMaxLength(EditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) { }

		[MissingMapper]
		public static void MapKeyboard(EditorHandler handler, IEditor editor) { }
	}

}