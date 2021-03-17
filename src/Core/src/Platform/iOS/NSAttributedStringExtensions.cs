using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public static class NSAttributedStringExtensions
	{
		public static NSMutableAttributedString? WithCharacterSpacing(this NSAttributedString attributedString, double characterSpacing)
		{
			if (attributedString == null || attributedString.Length == 0)
				return null;

			var attribute = attributedString.GetAttribute(UIStringAttributeKey.KerningAdjustment, 0, out _);

			// if we are going to un-set, but there is no adjustment, then bail out
			if (characterSpacing == 0 && attribute == null)
				return null;

			var mutableAttributedString = new NSMutableAttributedString(attributedString);
			mutableAttributedString.AddAttribute
			(
				UIStringAttributeKey.KerningAdjustment,
				NSObject.FromObject(characterSpacing),
				new NSRange(0, mutableAttributedString.Length)
			);
			return mutableAttributedString;
		}

		public static NSMutableAttributedString? WithLineHeight(this NSAttributedString attributedString, double lineHeight)
		{
			if (attributedString == null || attributedString.Length == 0)
				return null;

			var attribute = (NSParagraphStyle) attributedString.GetAttribute(UIStringAttributeKey.ParagraphStyle, 0, out _);

			// if we need to un-set the line height but there is no attribute to modify then we do nothing
			if (lineHeight == -1 && attribute == null)
				return null;

			var mutableParagraphStyle = new NSMutableParagraphStyle(attribute);
			mutableParagraphStyle.LineHeightMultiple = new nfloat(lineHeight >= 0 ? lineHeight : -1);

			var mutableAttributedString = new NSMutableAttributedString(attributedString);
			mutableAttributedString.AddAttribute
			(
				UIStringAttributeKey.ParagraphStyle,
				mutableParagraphStyle ,
				new NSRange(0, mutableAttributedString.Length)
			);
			return mutableAttributedString;
		}
	}
}