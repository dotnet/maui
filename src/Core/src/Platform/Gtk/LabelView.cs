using System;
using Cairo;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

#pragma warning disable CS0162 // Unreachable code detected

namespace Microsoft.Maui.Platform
{

	public class LabelView : Label
	{

		public float LineHeight { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		public LineBreakMode LineBreakMode { get; set; } = LineBreakMode.TailTruncation;

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			SetLayout(Layout, this);

			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);

			return;

			int Constrainted(int avail)
			{
				var wConstraint = orientation == Orientation.Horizontal ? avail : double.PositiveInfinity;
				var hConstraint = orientation == Orientation.Vertical ? avail : double.PositiveInfinity;
				var size = GetDesiredSize(wConstraint, hConstraint);

				return (int)(orientation == Orientation.Horizontal ? size.Width : size.Height);
			}

			natural_size = Constrainted(natural_size);
			minimum_size = Constrainted(minimum_size);
		}

		internal Size GetDesiredSize(double wConstraint, double hConstraint)
			=> GetDesiredSize(Layout, wConstraint, hConstraint,
				RequestMode == SizeRequestMode.HeightForWidth, Text, Lines,
				MarginStart, MarginTop, MarginEnd, MarginBottom);

		protected override bool OnDrawn(Context cr)
		{
			SetLayout(Layout, this);

			return base.OnDrawn(cr);
		}

		public static void SetLayout(Pango.Layout layout, LabelView platformView)
		{
			var horizontalTextAlignment = platformView.HorizontalTextAlignment.GetHorizontalAlignment();
			layout.Alignment = horizontalTextAlignment.ToPango();
			layout.Justify = horizontalTextAlignment.HasFlag(HorizontalAlignment.Justified);
			layout.Wrap = platformView.LineBreakMode.GetLineBreakMode().ToPangoWrap();
			layout.Ellipsize = platformView.LineBreakMode.GetLineBreakMode().ToPangoEllipsize();
			layout.LineSpacing = platformView.LineHeight > 1 ? platformView.LineHeight : 0;
			layout.FontDescription = platformView.GetPangoFontDescription();
		}

		public static void SetLayoutFromLabel(Pango.Layout layout, Label platformView)
		{
			layout.Ellipsize = platformView.Ellipsize;
			layout.Spacing = platformView.Layout.Spacing;
			layout.Attributes = platformView.Attributes;
		}

		public static Size GetDesiredSize(Pango.Layout layout, double widthConstraint, double heightConstraint, bool heightForWidth,
			string text, int lines, int marginStart, int marginTop, int marginEnd, int marginBottom)
		{
			int width = -1;
			int height = -1;

			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			var hMargin = marginStart + marginEnd;
			var vMargin = marginTop + marginBottom;

			var constraint = Math.Max(heightForWidth ? widthConstraint - hMargin : heightConstraint - vMargin,
				1);

			var lh = 0d;

			layout.Height = -1;
			layout.Width = -1;

			layout.SetText(text);

			if (!heightConstrained)
			{
				if (lines > 0)
				{
					lh = layout.GetLineHeigth(lines, false);
					layout.Height = (int)lh;
				}
			}
			
			if (!heightForWidth && heightConstrained && widthConstrained)
			{
				layout.Width = Math.Max((widthConstraint - hMargin).ScaledToPango(), -1);
			}

			var desiredSize = double.IsInfinity(constraint) ? -1 : constraint;

			if (desiredSize > 0)
			{
				if (heightForWidth)
					layout.Width = desiredSize.ScaledToPango();
				else
					layout.Height = desiredSize.ScaledToPango();
			
			}

			layout.SetText(text);
			layout.GetPixelSize(out var textWidth, out var textHeight);
			
			width = textWidth;
			height = textHeight;
			
			if (lh > 0)
			{
				height = Math.Min((int)lh.ScaledFromPango(), height);
			}

			width += hMargin;
			height += vMargin;

			return new Size(width, height);
		}

	}

}