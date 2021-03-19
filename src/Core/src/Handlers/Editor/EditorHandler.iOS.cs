using CoreGraphics;
using Foundation;
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

		protected virtual bool OnShouldChangeText(UITextView textView, NSRange range, string text)
		{
			var newLength = textView.Text?.Length + text.Length - range.Length;
			return newLength <= VirtualView?.MaxLength;
		}
	}
}