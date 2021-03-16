using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, UITextView>
	{
		static readonly int BaseHeight = 30;
		UILabel PlaceholderLabel { get; set; }

		protected override UITextView CreateNativeView()
		{
			var textView = new UITextView(CGRect.Empty);

			PlaceholderLabel = new UILabel
			{
				BackgroundColor = UIColor.Clear,
				Frame = new CGRect(0, 0, textView.Frame.Width, textView.Frame.Height),
				Lines = 0
			};

			CreatePlaceholderLabel(textView);

			return textView;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePlaceholder(editor);
		}

		void CreatePlaceholderLabel(UITextView textView)
		{
			textView.AddSubview(PlaceholderLabel);

			var edgeInsets = textView.TextContainerInset;
			var lineFragmentPadding = textView.TextContainer.LineFragmentPadding;

			var vConstraints = NSLayoutConstraint.FromVisualFormat(
				"V:|-" + edgeInsets.Top + "-[PlaceholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] {PlaceholderLabel}, new NSObject[] {new NSString("PlaceholderLabel")})
			);

			var hConstraints = NSLayoutConstraint.FromVisualFormat(
				"H:|-" + lineFragmentPadding + "-[PlaceholderLabel]-" + lineFragmentPadding + "-|",
				0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { PlaceholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
			);

			PlaceholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;
			// TODO: Add when we have CharacterSpacing
			// TODO: maybe put in extension method for setting these properties
			//PlaceholderLabel.AttributedText = PlaceholderLabel.AttributedText.AddCharacterSpacing(VirtualView.Placeholder, VirtualView.CharacterSpacing);

			textView.AddConstraints(hConstraints);
			textView.AddConstraints(vConstraints);
		}
	}
}
