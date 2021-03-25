using UIKit;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiTextView : UITextView
	{
		UILabel PlaceholderLabel { get; } = new UILabel
		{
			BackgroundColor = UIColor.Clear,
			Lines = 0
		};

		public MauiTextView(CGRect frame) : base(frame)
		{
			InitPlaceholderLabel();
		}

		public string? PlaceholderText
		{
			get => PlaceholderLabel.Text;
			set
			{
				PlaceholderLabel.Text = value;
				PlaceholderLabel.SizeToFit();
			}
		}

		public UIColor? PlaceholderTextColor
		{
			get => PlaceholderLabel.TextColor;
			set => PlaceholderLabel.TextColor = value;
		}

		public void HidePlaceholder(bool hide)
		{
			PlaceholderLabel.Hidden = hide;
		}

		void InitPlaceholderLabel()
		{
			AddSubview(PlaceholderLabel);

			var edgeInsets = TextContainerInset;
			var lineFragmentPadding = TextContainer.LineFragmentPadding;

			var vConstraints = NSLayoutConstraint.FromVisualFormat(
				"V:|-" + edgeInsets.Top + "-[PlaceholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { PlaceholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
			);

			var hConstraints = NSLayoutConstraint.FromVisualFormat(
				"H:|-" + lineFragmentPadding + "-[PlaceholderLabel]-" + lineFragmentPadding + "-|",
				0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { PlaceholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
			);

			PlaceholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;

			AddConstraints(hConstraints);
			AddConstraints(vConstraints);
		}
	}
}
