using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui
{
	public static class LabelExtensions
	{
		public static void UpdateText(this UILabel nativeLabel, ILabel label)
		{
			nativeLabel.Text = label.Text;
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
		}

		public static void UpdateHorizontalTextAlignment(this UILabel nativeLabel, ILabel label)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO ezhart Update this when FlowDirection is available 
			// (or update the extension to take an ILabel instead of an alignment and work it out from there) 
			nativeLabel.TextAlignment = label.HorizontalTextAlignment.ToNative(true);
		}

		public static void UpdateLineBreakMode(this UILabel nativeLabel, ILabel label)
		{
			SetLineBreakMode(nativeLabel, label);
		}

		public static void UpdateMaxLines(this UILabel nativeLabel, ILabel label)
		{
			int maxLines = label.MaxLines;

			if (maxLines >= 0)
			{
				nativeLabel.Lines = maxLines;
			}
		}

		public static void UpdatePadding(this MauiLabel nativeLabel, ILabel label)
		{
			nativeLabel.TextInsets = new UIEdgeInsets(
				(float)label.Padding.Top,
				(float)label.Padding.Left,
				(float)label.Padding.Bottom,
				(float)label.Padding.Right);
		}

		internal static void SetLineBreakMode(this UILabel nativeLabel, ILabel label)
		{
			int maxLines = label.MaxLines;
			if (maxLines < 0)
				maxLines = 0;

			switch (label.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					nativeLabel.LineBreakMode = UILineBreakMode.Clip;
					maxLines = 1;
					break;
				case LineBreakMode.WordWrap:
					nativeLabel.LineBreakMode = UILineBreakMode.WordWrap;
					break;
				case LineBreakMode.CharacterWrap:
					nativeLabel.LineBreakMode = UILineBreakMode.CharacterWrap;
					break;
				case LineBreakMode.HeadTruncation:
					nativeLabel.LineBreakMode = UILineBreakMode.HeadTruncation;
					maxLines = 1;
					break;
				case LineBreakMode.MiddleTruncation:
					nativeLabel.LineBreakMode = UILineBreakMode.MiddleTruncation;
					maxLines = 1;
					break;
				case LineBreakMode.TailTruncation:
					nativeLabel.LineBreakMode = UILineBreakMode.TailTruncation;
					maxLines = 1;
					break;
			}

			nativeLabel.Lines = maxLines;
		}

		public static void UpdateTextDecorations(this UILabel nativeLabel, ILabel label)
		{
			var modAttrText = nativeLabel.AttributedText?.WithDecorations(label.TextDecorations);

			if (modAttrText != null)
				nativeLabel.AttributedText = modAttrText;
		}

		internal static void UpdateLineHeight(this UILabel nativeLabel, ILabel label)
		{
			var modAttrText = nativeLabel.AttributedText?.WithLineHeight(label.LineHeight);

			if (modAttrText != null)
				nativeLabel.AttributedText = modAttrText;
		}
	}
}