using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiTextView : UITextView
	{
		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		public event EventHandler? TextSetOrChanged;

		internal UILabel PlaceholderLabel { get; } = new UILabel
		{
			BackgroundColor = UIColor.Clear,
			Lines = 0
		};

		public MauiTextView(CGRect frame) : base(frame)
		{
			InitPlaceholderLabel();
			this.Changed += OnChanged;
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

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
					HidePlaceholderIfTextIsPresent(value);
				}
			}
		}

		public override NSAttributedString AttributedText
		{
			get => base.AttributedText;
			set
			{
				var old = base.AttributedText;

				base.AttributedText = value;

				if (old?.Value != value?.Value)
				{
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
					HidePlaceholderIfTextIsPresent(value?.Value);
				}
			}
		}

		void HidePlaceholderIfTextIsPresent(string? value)
		{
			PlaceholderLabel.Hidden = !String.IsNullOrEmpty(value);
		}

		void OnChanged(object? sender, EventArgs e)
		{
			HidePlaceholderIfTextIsPresent(Text);
			TextSetOrChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
