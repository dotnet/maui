﻿using CoreGraphics;
using Foundation;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, MauiTextView>
	{
		static readonly int BaseHeight = 30;

		static readonly UIColor DefaultPlaceholderColor = ColorExtensions.PlaceholderColor;

		protected override MauiTextView CreateNativeView()
		{
			return new MauiTextView(CGRect.Empty);
		}

		protected override void SetupDefaults(MauiTextView nativeView)
		{
			nativeView.PlaceholderTextColor = DefaultPlaceholderColor;
		}

		protected override void ConnectHandler(MauiTextView nativeView)
		{
			nativeView.Changed += OnChanged;
			nativeView.ShouldChangeText += OnShouldChangeText;
		}

		protected override void DisconnectHandler(MauiTextView nativeView)
		{
			nativeView.Changed -= OnChanged;
			nativeView.ShouldChangeText -= OnShouldChangeText;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePlaceholderColor(editor, DefaultPlaceholderColor);
		}

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(editor);
		}

		public static void MapMaxLength(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateMaxLength(editor);
		}

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePredictiveText(editor);
		}

		public static void MapFormatting(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateMaxLength(editor);

			// Update all of the attributed text formatting properties
			handler.TypedNativeView?.UpdateCharacterSpacing(editor);
		}

		void OnChanged(object? sender, System.EventArgs e) => OnTextChanged();

		void OnTextChanged()
		{
			if (TypedNativeView == null)
				return;

			TypedNativeView.HidePlaceholder(!string.IsNullOrEmpty(TypedNativeView.Text));
		}

		bool OnShouldChangeText(UITextView textView, NSRange range, string replacementString)
		{
			var currLength = textView?.Text?.Length ?? 0;

			// fix a crash on undo
			if (range.Length + range.Location > currLength)
				return false;

			if (VirtualView == null || TypedNativeView == null)
				return false;

			var addLength = replacementString?.Length ?? 0;
			var remLength = range.Length;

			var newLength = currLength + addLength - remLength;

			return newLength <= VirtualView.MaxLength;
		}
	}
}