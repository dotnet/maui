using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, UITextView>
	{
		static readonly int BaseHeight = 30;
		UILabel PlaceholderLabel { get; set; }

		protected override UITextView CreateNativeView()
		{
			return new UITextView(CGRect.Empty);
		}

		protected override void SetupDefaults(UITextField nativeView)
		{
			if (PlaceholderLabel == null)
			{
				PlaceholderLabel = new UILabel
				{
					BackgroundColor = UIColor.Clear,
					Frame = new RectangleF(0, 0, Frame.Width, Frame.Height),
					Lines = 0
				};
			}

			CreatePlaceholderLabel();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			PlaceholderLabel.Text = Element.Placeholder;
			
			PlaceholderLabel.SizeToFit();
		}

		void CreatePlaceholderLabel()
		{
			if (TypedNativeView == null)
			{
				return;
			}

			if (VirtualView == null)
			{
				return;
			}

			TypedNativeView.AddSubview(PlaceholderLabel);

			var edgeInsets = TypedNativeView.TextContainerInset;
			var lineFragmentPadding = TypedNativeView.TextContainer.LineFragmentPadding;

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
			//PlaceholderLabel.AttributedText = PlaceholderLabel.AttributedText.AddCharacterSpacing(VirtualView.Placeholder, VirtualView.CharacterSpacing);

			TypedNativeView.AddConstraints(hConstraints);
			TypedNativeView.AddConstraints(vConstraints);
		}
	}
}
