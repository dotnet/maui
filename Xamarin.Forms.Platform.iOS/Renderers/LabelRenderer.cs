using System;
using System.ComponentModel;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Diagnostics;

#if __MOBILE__
using UIKit;
using NativeLabel = UIKit.UILabel;
#else
using AppKit;
using NativeLabel = AppKit.NSTextField;
#endif

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public class LabelRenderer : ViewRenderer<Label, NativeLabel>
	{
		SizeRequest _perfectSize;

		bool _perfectSizeValid;

		FormattedString _formatted;

		bool IsTextFormatted => _formatted != null;

		static HashSet<string> s_perfectSizeSet = new HashSet<string>
		{
			Label.TextProperty.PropertyName,
			Label.TextColorProperty.PropertyName,
			Label.FontProperty.PropertyName,
			Label.FormattedTextProperty.PropertyName,
			Label.LineBreakModeProperty.PropertyName,
			Label.LineHeightProperty.PropertyName,
			Label.PaddingProperty.PropertyName,
			Label.TextTypeProperty.PropertyName
		};

		[Internals.Preserve(Conditional = true)]
		public LabelRenderer()
		{

		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (!_perfectSizeValid)
			{
				_perfectSize = base.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				_perfectSize.Minimum = new Size(Math.Min(10, _perfectSize.Request.Width), _perfectSize.Request.Height);
				_perfectSizeValid = true;
			}

			var widthFits = widthConstraint >= _perfectSize.Request.Width;
			var heightFits = heightConstraint >= _perfectSize.Request.Height;

			if (widthFits && heightFits)
				return _perfectSize;

			var result = base.GetDesiredSize(widthConstraint, heightConstraint);
			var tinyWidth = Math.Min(10, result.Request.Width);
			result.Minimum = new Size(tinyWidth, result.Request.Height);

			if (widthFits || Element.LineBreakMode == LineBreakMode.NoWrap)
				return result;

			bool containerIsNotInfinitelyWide = !double.IsInfinity(widthConstraint);

			if (containerIsNotInfinitelyWide)
			{
				bool textCouldHaveWrapped = Element.LineBreakMode == LineBreakMode.WordWrap || Element.LineBreakMode == LineBreakMode.CharacterWrap;
				bool textExceedsContainer = result.Request.Width > widthConstraint;

				if (textExceedsContainer || textCouldHaveWrapped)
				{
					var expandedWidth = Math.Max(tinyWidth, widthConstraint);
					result.Request = new Size(expandedWidth, result.Request.Height);
				}
			}

			return result;
		}

#if __MOBILE__
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
#else
		public override void Layout()
		{
			base.Layout();
#endif

			if (Control == null)
				return;

			SizeF fitSize;
			nfloat labelHeight;
			switch (Element.VerticalTextAlignment)
			{
				case TextAlignment.Start:
					fitSize = Control.SizeThatFits(Element.Bounds.Size.ToSizeF());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, labelHeight);
					break;
				case TextAlignment.Center:

#if __MOBILE__
					Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
#else
					fitSize = Control.SizeThatFits(Element.Bounds.Size.ToSizeF());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					var yOffset = (int)(Element.Height / 2 - labelHeight / 2);
					Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, (nfloat)Element.Height - yOffset);
#endif
					break;
				case TextAlignment.End:
					fitSize = Control.SizeThatFits(Element.Bounds.Size.ToSizeF());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
#if __MOBILE__
					nfloat yOffset = 0;
					yOffset = (nfloat)(Element.Height - labelHeight);
					Control.Frame = new RectangleF(0, yOffset, (nfloat)Element.Width, labelHeight);
#else
					Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, labelHeight);
#endif
					break;
			}

			Control.RecalculateSpanPositions(Element);

		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (Element != null)
				{
					Element.PropertyChanging -= ElementPropertyChanging;
				}
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			_perfectSizeValid = false;

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanging -= ElementPropertyChanging;
			}

			if (e.NewElement != null)
			{
				e.NewElement.PropertyChanging += ElementPropertyChanging;
				if (Control == null)
				{
					SetNativeControl(CreateNativeControl());
#if !__MOBILE__
					Control.Editable = false;
					Control.Bezeled = false;
					Control.DrawsBackground = false;
#endif
				}

				UpdateLineBreakMode();
				UpdateHorizontalTextAlignment();
				UpdateText();
				UpdateTextDecorations();
				UpdateTextColor();
				UpdateFont();
				UpdateMaxLines();
				UpdateCharacterSpacing();
				UpdatePadding();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateLayout();
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Label.TextProperty.PropertyName)
			{
				UpdateText();
				UpdateTextDecorations();
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == Label.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Label.TextDecorationsProperty.PropertyName)
				UpdateTextDecorations();
			else if (e.PropertyName == Label.FormattedTextProperty.PropertyName)
			{
				UpdateText();
				UpdateTextDecorations();
			}
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Label.LineHeightProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.MaxLinesProperty.PropertyName)
				UpdateMaxLines();
			else if (e.PropertyName == Label.PaddingProperty.PropertyName)
				UpdatePadding();
			else if (e.PropertyName == Label.TextTypeProperty.PropertyName)
				UpdateText();
		}

		protected override NativeLabel CreateNativeControl()
		{
#if __MOBILE__
			return Element.Padding.IsEmpty ? new NativeLabel(RectangleF.Empty) : new FormsLabel(RectangleF.Empty);
#else
			return new NativeLabel(RectangleF.Empty);
#endif
		}

		void ElementPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (s_perfectSizeSet.Contains(e.PropertyName))
				_perfectSizeValid = false;
		}

		void UpdateTextDecorations()
		{
			if (IsElementOrControlEmpty)
				return;

			if (Element?.TextType != TextType.Text)
				return;

			if (!Element.IsSet(Label.TextDecorationsProperty))
				return;

#if __MOBILE__
			if (!(Control.AttributedText?.Length > 0))
				return;
#else
			if (!(Control.AttributedStringValue?.Length > 0))
				return;
#endif

			var textDecorations = Element.TextDecorations;
#if __MOBILE__
			var newAttributedText = new NSMutableAttributedString(Control.AttributedText);
			var strikeThroughStyleKey = UIStringAttributeKey.StrikethroughStyle;
			var underlineStyleKey = UIStringAttributeKey.UnderlineStyle;

#else
			var newAttributedText = new NSMutableAttributedString(Control.AttributedStringValue);
			var strikeThroughStyleKey = NSStringAttributeKey.StrikethroughStyle;
			var underlineStyleKey = NSStringAttributeKey.UnderlineStyle;
#endif
			var range = new NSRange(0, newAttributedText.Length);

			if ((textDecorations & TextDecorations.Strikethrough) == 0)
				newAttributedText.RemoveAttribute(strikeThroughStyleKey, range);
			else
				newAttributedText.AddAttribute(strikeThroughStyleKey, NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

			if ((textDecorations & TextDecorations.Underline) == 0)
				newAttributedText.RemoveAttribute(underlineStyleKey, range);
			else
				newAttributedText.AddAttribute(underlineStyleKey, NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

#if !__MOBILE__
			Control.AttributedStringValue = newAttributedText;
#endif
			UpdateCharacterSpacing();
			_perfectSizeValid = false;
		}

#if __MOBILE__
		protected override void SetAccessibilityLabel()
		{
			// If we have not specified an AccessibilityLabel and the AccessibiltyLabel is current bound to the Text,
			// exit this method so we don't set the AccessibilityLabel value and break the binding.
			// This may pose a problem for users who want to explicitly set the AccessibilityLabel to null, but this
			// will prevent us from inadvertently breaking UI Tests that are using Query.Marked to get the dynamic Text 
			// of the Label.

			var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);
			if (string.IsNullOrWhiteSpace(elemValue) && Control?.AccessibilityLabel == Control?.Text)
				return;

			base.SetAccessibilityLabel();
		}
#endif

		protected override void SetBackgroundColor(Color color)
		{
#if __MOBILE__
			if (color == Color.Default)
				BackgroundColor = UIColor.Clear;
			else
				BackgroundColor = color.ToUIColor();
#else
			if (color == Color.Default)
				Layer.BackgroundColor = NSColor.Clear.CGColor;
			else
				Layer.BackgroundColor = color.ToCGColor();
#endif

		}

		void UpdateHorizontalTextAlignment()
		{
#if __MOBILE__
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
#else
			Control.Alignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
#endif
		}

		void UpdateLineBreakMode()
		{
#if __MOBILE__
			switch (Element.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					Control.LineBreakMode = UILineBreakMode.Clip;
					break;
				case LineBreakMode.WordWrap:
					Control.LineBreakMode = UILineBreakMode.WordWrap;
					break;
				case LineBreakMode.CharacterWrap:
					Control.LineBreakMode = UILineBreakMode.CharacterWrap;
					break;
				case LineBreakMode.HeadTruncation:
					Control.LineBreakMode = UILineBreakMode.HeadTruncation;
					break;
				case LineBreakMode.MiddleTruncation:
					Control.LineBreakMode = UILineBreakMode.MiddleTruncation;
					break;
				case LineBreakMode.TailTruncation:
					Control.LineBreakMode = UILineBreakMode.TailTruncation;
					break;
			}
#else
			switch (Element.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					Control.LineBreakMode = NSLineBreakMode.Clipping;
					break;
				case LineBreakMode.WordWrap:
					Control.LineBreakMode = NSLineBreakMode.ByWordWrapping;
					break;
				case LineBreakMode.CharacterWrap:
					Control.LineBreakMode = NSLineBreakMode.CharWrapping;
					break;
				case LineBreakMode.HeadTruncation:
					Control.LineBreakMode = NSLineBreakMode.TruncatingHead;
					break;
				case LineBreakMode.MiddleTruncation:
					Control.LineBreakMode = NSLineBreakMode.TruncatingMiddle;
					break;
				case LineBreakMode.TailTruncation:
					Control.LineBreakMode = NSLineBreakMode.TruncatingTail;
					break;
			}
#endif
		}

		void UpdateCharacterSpacing()
		{
			if (IsElementOrControlEmpty)
				return;

			if (Element?.TextType != TextType.Text)
				return;
#if __MOBILE__
			var textAttr = Control.AttributedText.AddCharacterSpacing(Element.Text, Element.CharacterSpacing);

			if (textAttr != null)
				Control.AttributedText = textAttr;
#else
   			var textAttr = Control.AttributedStringValue.AddCharacterSpacing(Element.Text, Element.CharacterSpacing);

			if (textAttr != null)
				Control.AttributedStringValue = textAttr;
#endif

			_perfectSizeValid = false;
		}

		void UpdateText()
		{
            if (IsElementOrControlEmpty)
                return;

            switch (Element.TextType)
			{
				case TextType.Html:
					UpdateTextHtml();
					break;

				default:
					UpdateTextPlainText();
					break;
			}
		}

		void UpdateTextPlainText()
		{
			_formatted = Element.FormattedText;
			if (_formatted == null && Element.LineHeight >= 0)
				_formatted = Element.Text;

			if (IsTextFormatted)
			{
				UpdateFormattedText();
			}
			else
			{
#if __MOBILE__
				Control.Text = Element.Text;
#else
				Control.StringValue = Element.Text ?? "";
#endif
			}
			UpdateLayout();
		}

		void UpdateFormattedText()
		{
#if __MOBILE__
			Control.AttributedText = _formatted.ToAttributed(Element, Element.TextColor, Element.HorizontalTextAlignment, Element.LineHeight);
#else
			Control.AttributedStringValue = _formatted.ToAttributed(Element, Element.TextColor, Element.HorizontalTextAlignment, Element.LineHeight);
#endif
			_perfectSizeValid = false;
		}

		void UpdateTextHtml()
		{
			if (IsElementOrControlEmpty)
				return;

			string text = Element.Text ?? string.Empty;

			var attr = GetNSAttributedStringDocumentAttributes();
#if __MOBILE__

			NSError nsError = null;

			Control.AttributedText = new NSAttributedString(text, attr, ref nsError);
#else
			var htmlData = new NSMutableData();
			htmlData.SetData(text);

			Control.AttributedStringValue = new NSAttributedString(htmlData, attr, out _);
#endif
			_perfectSizeValid = false;
		}

		protected virtual NSAttributedStringDocumentAttributes GetNSAttributedStringDocumentAttributes()
		{
			return new NSAttributedStringDocumentAttributes
			{
				DocumentType = NSDocumentType.HTML,
				StringEncoding = NSStringEncoding.UTF8
			};
		}

		void UpdateFont()
		{
			if (Element?.TextType != TextType.Text)
				return;

			if (IsTextFormatted)
			{
				UpdateFormattedText();
				return;
			}

#if __MOBILE__
			Control.Font = Element.ToUIFont();
#else
			Control.Font = Element.ToNSFont();
#endif
			UpdateLayout();
		}

		void UpdateTextColor()
		{
			if (Element?.TextType != TextType.Text)
				return;

			if (IsTextFormatted)
			{
				UpdateFormattedText();
				return;
			}

			var textColor = (Color)Element.GetValue(Label.TextColorProperty);

			// default value of color documented to be black in iOS docs
#if __MOBILE__
			Control.TextColor = textColor.ToUIColor(ColorExtensions.Black);
#else
			var alignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
			var textWithColor = new NSAttributedString(Element.Text ?? "", font: Element.ToNSFont(), foregroundColor: textColor.ToNSColor(ColorExtensions.Black), paragraphStyle: new NSMutableParagraphStyle() { Alignment = alignment });
			textWithColor = textWithColor.AddCharacterSpacing(Element.Text ?? string.Empty, Element.CharacterSpacing);
			Control.AttributedStringValue = textWithColor;
#endif
			UpdateLayout();
		}
		void UpdateLayout()
		{
#if __MOBILE__
			LayoutSubviews();
#else
			Layout();
#endif
		}

		void UpdateMaxLines()
		{
			if (Element.MaxLines >= 0)
			{
#if __MOBILE__
				Control.Lines = Element.MaxLines;

				LayoutSubviews();
#else
				Control.MaximumNumberOfLines = Element.MaxLines;

				Layout();
#endif
			}
			else
			{
#if __MOBILE__
				switch (Element.LineBreakMode)
				{
					case LineBreakMode.WordWrap:
					case LineBreakMode.CharacterWrap:
						Control.Lines = 0;
						break;
					case LineBreakMode.NoWrap:
					case LineBreakMode.HeadTruncation:
					case LineBreakMode.MiddleTruncation:
					case LineBreakMode.TailTruncation:
						Control.Lines = 1;
						break;
				}

				LayoutSubviews();
#else
				switch (Element.LineBreakMode)
				{
					case LineBreakMode.WordWrap:
					case LineBreakMode.CharacterWrap:
						Control.MaximumNumberOfLines = 0;
						break;
					case LineBreakMode.NoWrap:
					case LineBreakMode.HeadTruncation:
					case LineBreakMode.MiddleTruncation:
					case LineBreakMode.TailTruncation:
						Control.MaximumNumberOfLines = 1;
						break;
				}

				Layout();
#endif
			}
		}

		void UpdatePadding()
		{
			if (IsElementOrControlEmpty)
				return;

			if (Element.Padding.IsEmpty)
				return;

#if __MOBILE__
			var formsLabel = Control as FormsLabel;
			if (formsLabel == null)
			{
				Debug.WriteLine($"{nameof(LabelRenderer)}: On iOS, a Label created with no padding will ignore padding changes");
				return;
			}

			formsLabel.TextInsets = new UIEdgeInsets(
					(float)Element.Padding.Top,
					(float)Element.Padding.Left,
					(float)Element.Padding.Bottom,
					(float)Element.Padding.Right);
			UpdateLayout();
#endif
		}

#if __MOBILE__
		class FormsLabel : NativeLabel
		{
			public UIEdgeInsets TextInsets { get; set; }

			public FormsLabel(RectangleF frame) : base(frame)
			{
			}

			public override void DrawText(RectangleF rect) => base.DrawText(TextInsets.InsetRect(rect));

			public override SizeF SizeThatFits(SizeF size) => AddInsets(base.SizeThatFits(size));

			SizeF AddInsets(SizeF size) => new SizeF(
				width: size.Width + TextInsets.Left + TextInsets.Right,
				height: size.Height + TextInsets.Top + TextInsets.Bottom);
		}
#endif
	}
}
