using System;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiTextView : UITextView, IMauiTextView
	{
		readonly UILabel _placeholderLabel;
		bool _textChanges;

		public MauiTextView()
		{
			_placeholderLabel = InitPlaceholderLabel();
			_textChanges = false;

			Changed += OnChanged;
		}

		public MauiTextView(CGRect frame)
			: base(frame)
		{
			_placeholderLabel = InitPlaceholderLabel();
			Changed += OnChanged;
		}

		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		public event EventHandler? TextSetOrChanged;

		public event EventHandler? FrameChanged;

		public string? PlaceholderText
		{
			get => _placeholderLabel.Text;
			set
			{
				_placeholderLabel.Text = value;
				_placeholderLabel.SizeToFit();
			}
		}

		public NSAttributedString? AttributedPlaceholderText
		{
			get => _placeholderLabel.AttributedText;
			set
			{
				_placeholderLabel.AttributedText = value;
				_placeholderLabel.SizeToFit();
			}
		}

		public UIColor? PlaceholderTextColor
		{
			get => _placeholderLabel.TextColor;
			set => _placeholderLabel.TextColor = value;
		}

		public override CGRect Frame
		{
			get => base.Frame;
			set
			{
				base.Frame = value;
				FrameChanged?.Invoke(this, EventArgs.Empty);

				// When a new line is added to the UITextView the resize happens after the view has already scrolled
				// This causes the view to reposition without the scroll. If TextChanges is enabled then the Frame
				// will resize until it can't anymore and thus it should never be scrolled until the Frame can't increase in size
				if (_textChanges)
					ScrollRangeToVisible(new NSRange(0, 0));		
			}
		}
    
		public TextAlignment VerticalTextAlignment { get; set; }

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					HidePlaceholderIfTextIsPresent(value);
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
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
					HidePlaceholderIfTextIsPresent(value?.Value);
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public void UpdateShouldChangeScrollPosition(bool textChanges)
		{
			_textChanges = textChanges;
    }
    
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			ShouldCenterVertically();
		}

		UILabel InitPlaceholderLabel()
		{
			var placeholderLabel = new UILabel
			{
				BackgroundColor = UIColor.Clear,
				TextColor = ColorExtensions.PlaceholderColor,
				Lines = 0
			};

			AddSubview(placeholderLabel);

			var edgeInsets = TextContainerInset;
			var lineFragmentPadding = TextContainer.LineFragmentPadding;

			var vConstraints = NSLayoutConstraint.FromVisualFormat(
				"V:|-" + edgeInsets.Top + "-[PlaceholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
			);

			var hConstraints = NSLayoutConstraint.FromVisualFormat(
				"H:|-" + lineFragmentPadding + "-[PlaceholderLabel]-" + lineFragmentPadding + "-|",
				0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
			);

			placeholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;

			AddConstraints(hConstraints);
			AddConstraints(vConstraints);

			return placeholderLabel;
		}

		void HidePlaceholderIfTextIsPresent(string? value)
		{
			_placeholderLabel.Hidden = !string.IsNullOrEmpty(value);
		}

		void OnChanged(object? sender, EventArgs e)
		{
			HidePlaceholderIfTextIsPresent(Text);
			TextSetOrChanged?.Invoke(this, EventArgs.Empty);
		}

		void ShouldCenterVertically()
		{
			var fittingSize = new CGSize(Bounds.Width, NFloat.MaxValue);
			var sizeThatFits = SizeThatFits(fittingSize);
			var availableSpace = (Bounds.Height - sizeThatFits.Height * ZoomScale);
			ContentOffset = VerticalTextAlignment switch
			{
				Maui.TextAlignment.Center => new CGPoint(0, -Math.Max(1, availableSpace / 2)),
				Maui.TextAlignment.End => new CGPoint(0, -Math.Max(1, availableSpace)),
				_ => new CGPoint(0, 0),
			};
		}
	}
}