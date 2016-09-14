using System;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class LabelRenderer : ViewRenderer<Label, UILabel>
	{
		SizeRequest _perfectSize;

		bool _perfectSizeValid;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (!_perfectSizeValid)
			{
				_perfectSize = base.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				_perfectSize.Minimum = new Size(Math.Min(10, _perfectSize.Request.Width), _perfectSize.Request.Height);
				_perfectSizeValid = true;
			}

			if (widthConstraint >= _perfectSize.Request.Width && heightConstraint >= _perfectSize.Request.Height)
				return _perfectSize;

			var result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(Math.Min(10, result.Request.Width), result.Request.Height);
			if ((Element.LineBreakMode & (LineBreakMode.TailTruncation | LineBreakMode.HeadTruncation | LineBreakMode.MiddleTruncation)) != 0)
			{
				if (result.Request.Width > widthConstraint)
					result.Request = new Size(Math.Max(result.Minimum.Width, widthConstraint), result.Request.Height);
			}

			return result;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
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
					Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
					break;
				case TextAlignment.End:
					nfloat yOffset = 0;
					fitSize = Control.SizeThatFits(Element.Bounds.Size.ToSizeF());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					yOffset = (nfloat)(Element.Height - labelHeight);
					Control.Frame = new RectangleF(0, yOffset, (nfloat)Element.Width, labelHeight);
					break;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new UILabel(RectangleF.Empty) { BackgroundColor = UIColor.Clear });
				}

				UpdateText();
				UpdateLineBreakMode();
				UpdateAlignment();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				LayoutSubviews();
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FormattedTextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (color == Color.Default)
				BackgroundColor = UIColor.Clear;
			else
				BackgroundColor = color.ToUIColor();
		}

		void UpdateAlignment()
		{
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateLineBreakMode()
		{
			_perfectSizeValid = false;

			switch (Element.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					Control.LineBreakMode = UILineBreakMode.Clip;
					Control.Lines = 1;
					break;
				case LineBreakMode.WordWrap:
					Control.LineBreakMode = UILineBreakMode.WordWrap;
					Control.Lines = 0;
					break;
				case LineBreakMode.CharacterWrap:
					Control.LineBreakMode = UILineBreakMode.CharacterWrap;
					Control.Lines = 0;
					break;
				case LineBreakMode.HeadTruncation:
					Control.LineBreakMode = UILineBreakMode.HeadTruncation;
					Control.Lines = 1;
					break;
				case LineBreakMode.MiddleTruncation:
					Control.LineBreakMode = UILineBreakMode.MiddleTruncation;
					Control.Lines = 1;
					break;
				case LineBreakMode.TailTruncation:
					Control.LineBreakMode = UILineBreakMode.TailTruncation;
					Control.Lines = 1;
					break;
			}
		}

		void UpdateText()
		{
			_perfectSizeValid = false;

			var values = Element.GetValues(Label.FormattedTextProperty, Label.TextProperty, Label.TextColorProperty);
			var formatted = (FormattedString)values[0];
			if (formatted != null)
				Control.AttributedText = formatted.ToAttributed(Element, (Color)values[2]);
			else
			{
				Control.Text = (string)values[1];
				// default value of color documented to be black in iOS docs
				Control.Font = Element.ToUIFont();
				Control.TextColor = ((Color)values[2]).ToUIColor(ColorExtensions.Black);
			}

			LayoutSubviews();
		}
	}
}