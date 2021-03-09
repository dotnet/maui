using AppKit;
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
    internal static class NSAttributedStringExtensions
    {
        internal static NSMutableAttributedString AddCharacterSpacing(this NSMutableAttributedString attributedString, string text, double characterSpacing)
        {
            if (attributedString == null || attributedString.Length == 0)
            {
                attributedString = text == null ? new NSMutableAttributedString() : new NSMutableAttributedString(text);
            }
            else
            {
                attributedString = new NSMutableAttributedString(attributedString);
            }

            AddKerningAdjustment(attributedString, text, characterSpacing);

            return attributedString;
        }

        internal static NSMutableAttributedString AddCharacterSpacing(this NSAttributedString attributedString, string text, double characterSpacing)
        {
            NSMutableAttributedString mutableAttributedString;
            if (attributedString == null || attributedString.Length == 0)
            {
                mutableAttributedString = text == null ? new NSMutableAttributedString() : new NSMutableAttributedString(text);
            }
            else
            {
                mutableAttributedString = new NSMutableAttributedString(attributedString);
            }

            AddKerningAdjustment(mutableAttributedString, text, characterSpacing);

            return mutableAttributedString;
        }

        internal static void AddKerningAdjustment(NSMutableAttributedString mutableAttributedString, string text, double characterSpacing)
        {
            if (!string.IsNullOrEmpty(text))
            {
                mutableAttributedString.AddAttribute
                (
                    NSStringAttributeKey.KerningAdjustment,
                    NSObject.FromObject(characterSpacing), new NSRange(0, text.Length - 1)
                );
            }
        }
    }
}