using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiTextView>
	{
		static readonly int BaseHeight = 30;

		static readonly UIColor DefaultPlaceholderColor = ColorExtensions.PlaceholderColor;

		protected override MauiTextView CreateNativeView()
		{
			return new MauiTextView(CGRect.Empty);
		}

		protected override void ConnectHandler(MauiTextView nativeView)
		{
			nativeView.Changed += OnChanged;
			nativeView.ShouldChangeText += OnShouldChangeText;
			nativeView.Ended += OnEnded;
		}

		protected override void DisconnectHandler(MauiTextView nativeView)
		{
			nativeView.Changed -= OnChanged;
			nativeView.ShouldChangeText -= OnShouldChangeText;
			nativeView.Ended -= OnEnded;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

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
			handler.NativeView?.UpdatePlaceholderColor(editor, DefaultPlaceholderColor);
		}

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateCharacterSpacing(editor);
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
			handler.NativeView?.UpdatePredictiveText(editor);
		}

		public static void MapFormatting(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateMaxLength(editor);

			// Update all of the attributed text formatting properties
			handler.NativeView?.UpdateCharacterSpacing(editor);
		}

		public static void MapFont(EditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(editor, fontManager);
		}

		void OnChanged(object? sender, System.EventArgs e) => OnTextChanged();

		void OnTextChanged()
		{
			if (NativeView == null)
				return;

			NativeView.HidePlaceholder(!string.IsNullOrEmpty(NativeView.Text));
		}

		bool OnShouldChangeText(UITextView textView, NSRange range, string replacementString)
		{
			var currLength = textView?.Text?.Length ?? 0;

			// Fix a crash on undo
			if (range.Length + range.Location > currLength)
				return false;

			if (VirtualView == null || NativeView == null)
				return false;

			var addLength = replacementString?.Length ?? 0;
			var remLength = range.Length;

			var newLength = currLength + addLength - remLength;

			return newLength <= VirtualView.MaxLength;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (VirtualView == null || NativeView == null)
				return;

			if (NativeView.Text != VirtualView.Text)
				VirtualView.Text = NativeView.Text ?? string.Empty;

			// TODO: Update IsFocused property
			VirtualView.Completed();
		}
	}
}