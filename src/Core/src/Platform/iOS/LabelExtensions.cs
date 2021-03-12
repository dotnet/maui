using Foundation;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui
{
	public static class LabelExtensions
	{
		public static void UpdateText(this UILabel nativeLabel, ILabel label)
		{
			nativeLabel.Text = label.Text;

			nativeLabel.UpdateCharacterSpacing(label);
		}

		public static void UpdateTextColor(this UILabel nativeLabel, ILabel label)
		{
			var textColor = label.TextColor;

			if (textColor.IsDefault)
			{
				// Default value of color documented to be black in iOS docs
				nativeLabel.TextColor = textColor.ToNative(ColorExtensions.LabelColor);
			}
			else
			{
				nativeLabel.TextColor = textColor.ToNative(textColor);
			}
		}

		public static void UpdateCharacterSpacing(this UILabel nativeLabel, ILabel label)
		{
			if (string.IsNullOrEmpty(label.Text))
				return;

			var textAttr = nativeLabel.AttributedText?.WithCharacterSpacing(label.CharacterSpacing);

			if (textAttr != null)
				nativeLabel.AttributedText = textAttr;
		}

		public static void UpdateFont(this UILabel nativeLabel, ILabel label, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(label.Font);
			nativeLabel.Font = uiFont;

			nativeLabel.UpdateCharacterSpacing(label);
		}

		public static void UpdatePadding(this MauiLabel nativeLabel, ILabel label)
		{
			nativeLabel.TextInsets = new UIEdgeInsets(
				(float)label.Padding.Top,
				(float)label.Padding.Left,
				(float)label.Padding.Bottom,
				(float)label.Padding.Right);
		}

		public static void UpdateTextDecorations(this UILabel nativeLabel, ILabel label)
		{
			if (nativeLabel.AttributedText != null && !(nativeLabel.AttributedText?.Length > 0))
				return;

			var textDecorations = label?.TextDecorations;

			var newAttributedText = nativeLabel.AttributedText != null ? new NSMutableAttributedString(nativeLabel.AttributedText) : new NSMutableAttributedString(label?.Text ?? string.Empty);
			var strikeThroughStyleKey = UIStringAttributeKey.StrikethroughStyle;
			var underlineStyleKey = UIStringAttributeKey.UnderlineStyle;

			var range = new NSRange(0, newAttributedText.Length);

			if ((textDecorations & TextDecorations.Strikethrough) == 0)
				newAttributedText.RemoveAttribute(strikeThroughStyleKey, range);
			else
				newAttributedText.AddAttribute(strikeThroughStyleKey, NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

			if ((textDecorations & TextDecorations.Underline) == 0)
				newAttributedText.RemoveAttribute(underlineStyleKey, range);
			else
				newAttributedText.AddAttribute(underlineStyleKey, NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

			nativeLabel.AttributedText = newAttributedText;
		}
	}
}