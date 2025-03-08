using System;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateTextColor(this UILabel platformLabel, ITextStyle textStyle, UIColor? defaultColor = null)
		{
			// Default value of color documented to be black in iOS docs
			var textColor = textStyle.TextColor;
			platformLabel.TextColor = textColor.ToPlatform(defaultColor ?? ColorExtensions.LabelColor);
		}

		public static void UpdateCharacterSpacing(this UILabel platformLabel, ITextStyle textStyle)
		{
			var textAttr = platformLabel.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);

			if (textAttr != null)
				platformLabel.AttributedText = textAttr;
		}

		public static void UpdateFont(this UILabel platformLabel, ITextStyle textStyle, IFontManager fontManager) =>
			platformLabel.UpdateFont(textStyle, fontManager, UIFont.LabelFontSize);

		public static void UpdateFont(this UILabel platformLabel, ITextStyle textStyle, IFontManager fontManager, double defaultSize)
		{
			var uiFont = fontManager.GetFont(textStyle.Font, defaultSize);
			platformLabel.Font = uiFont;
		}

		public static void UpdateHorizontalTextAlignment(this UILabel platformLabel, ILabel label)
		{
			platformLabel.TextAlignment = label.HorizontalTextAlignment.ToPlatformHorizontal(platformLabel.EffectiveUserInterfaceLayoutDirection);
		}

		// Don't use this method, it doesn't work. But we can't remove it.
		public static void UpdateVerticalTextAlignment(this UILabel platformLabel, ILabel label)
		{
			if (!platformLabel.Bounds.IsEmpty)
				platformLabel.InvalidateMeasure(label);
		}

		internal static void UpdateVerticalTextAlignment(this MauiLabel platformLabel, ILabel label)
		{
			platformLabel.VerticalAlignment = label.VerticalTextAlignment.ToPlatformVertical();
		}

		public static void UpdatePadding(this MauiLabel platformLabel, ILabel label)
		{
			platformLabel.TextInsets = new UIEdgeInsets(
				(float)label.Padding.Top,
				(float)label.Padding.Left,
				(float)label.Padding.Bottom,
				(float)label.Padding.Right);
		}

		public static void UpdateTextDecorations(this UILabel platformLabel, ILabel label)
		{
			var modAttrText = platformLabel.AttributedText?.WithDecorations(label.TextDecorations);

			if (modAttrText != null)
				platformLabel.AttributedText = modAttrText;
		}

		public static void UpdateLineHeight(this UILabel platformLabel, ILabel label)
		{
			var modAttrText = platformLabel.AttributedText?.WithLineHeight(label.LineHeight);

			if (modAttrText != null)
				platformLabel.AttributedText = modAttrText;
		}

		internal static void UpdateTextHtml(this UILabel platformLabel, ILabel label)
		{
			string text = label.Text ?? string.Empty;

			var attr = new NSAttributedStringDocumentAttributes
			{
				DocumentType = NSDocumentType.HTML,
#if IOS17_5_OR_GREATER || MACCATALYST17_5_OR_GREATER
				CharacterEncoding = NSStringEncoding.UTF8
#else
				StringEncoding = NSStringEncoding.UTF8
#endif
			};

			NSError nsError = new();
#pragma warning disable CS8601
			platformLabel.AttributedText = new NSAttributedString(text, attr, ref nsError);
#pragma warning restore CS8601
  			platformLabel.UserInteractionEnabled = true;
			platformLabel.DetectAndOpenLink();
        }
		internal static void RemoveCurrentGesture(this UILabel platformLabel)
		{
			if(platformLabel.GestureRecognizers is null)
				return;
			foreach(var gesture in platformLabel.GestureRecognizers)
			{
				if(gesture is HtmlTextGestureRecognizer htmlTextGesture)
				{
					platformLabel.RemoveGestureRecognizer(htmlTextGesture);
					break;
				}
			}
		}

		internal static void DetectAndOpenLink(this UILabel platformLabel)
		{
			platformLabel.RemoveCurrentGesture();
			 var tapGesture = new HtmlTextGestureRecognizer((UITapGestureRecognizer recognizer) =>
			 {
			 	if (recognizer.State != UIGestureRecognizerState.Recognized) return;
			    if (platformLabel.AttributedText is null) return;

				var layoutManager = new NSLayoutManager();
				var textStorage = new NSTextStorage();
				var textContainer = new NSTextContainer();

				textStorage.SetString(platformLabel.AttributedText);
				layoutManager.AddTextContainer(textContainer);
				textStorage.AddLayoutManager(layoutManager);
				
				textContainer.LineFragmentPadding = 0;
				textContainer.LineBreakMode = platformLabel.LineBreakMode;
				textContainer.Size = platformLabel.Bounds.Size;

				var location = recognizer.LocationInView(platformLabel);
				var index = (nint)layoutManager.GetCharacterIndex(location, textContainer);
				
				if (index < platformLabel.AttributedText.Length)
				{
					var url = platformLabel.AttributedText.GetAttribute(UIStringAttributeKey.Link, index, out _);
					if (url is NSUrl nsUrl)
					{
						UIApplication.SharedApplication.OpenUrl(nsUrl,new UIApplicationOpenUrlOptions(),null);
					}
				}
			 });
			 platformLabel.AddGestureRecognizer(tapGesture);	
		}

		internal static void UpdateTextPlainText(this UILabel platformLabel, IText label)
		{
			platformLabel.Text = label.Text;
		}
	}

	internal class HtmlTextGestureRecognizer:  UITapGestureRecognizer
	{
		public HtmlTextGestureRecognizer(Action<UITapGestureRecognizer> action):base(action)
		{
				CancelsTouchesInView = false;
				ShouldRecognizeSimultaneously = GetRecognizeSimultaneously;
		}

		private bool GetRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer) => true;
	}
}
