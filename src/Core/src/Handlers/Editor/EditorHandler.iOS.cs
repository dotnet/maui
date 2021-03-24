using CoreGraphics;
using Foundation;
using System;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, UITextView>
	{
		static readonly int BaseHeight = 30;

		protected override UITextView CreateNativeView()
		{
			return new UITextView(CGRect.Empty);
		}

		protected override void ConnectHandler(UITextView nativeView)
		{
			nativeView.ShouldChangeText += OnShouldChangeText;
		}

		protected override void DisconnectHandler(UITextView nativeView)
		{
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

		public static void MapFont(EditorHandler handler, IEditor editor)
		{
			var services = handler.Services ??
				throw new InvalidOperationException($"Unable to find service provider, the App.Current.Services was null.");
			var fontManager = services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(editor, fontManager);
		}
	}
}