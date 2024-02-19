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

		protected  void _OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			if (LineHeight > 1)
				Layout.LineSpacing = LineHeight;

			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);
			var wContraint = orientation == Orientation.Horizontal ? natural_size : double.PositiveInfinity;
			var hContraint = orientation == Orientation.Vertical ? natural_size : double.PositiveInfinity;
			var size = GetDesiredSize(wContraint, hContraint, this.RequestMode == SizeRequestMode.HeightForWidth);
			natural_size = (int)(orientation == Orientation.Horizontal ? size.Width : size.Height);

			wContraint = orientation == Orientation.Horizontal ? minimum_size : double.PositiveInfinity;
			hContraint = orientation == Orientation.Vertical ? minimum_size : double.PositiveInfinity;
			size = GetDesiredSize(wContraint, hContraint, this.RequestMode == SizeRequestMode.HeightForWidth);
			minimum_size = (int)(orientation == Orientation.Horizontal ? size.Width : size.Height);
		}

		protected override bool OnDrawn(Context cr)
		{
			if (LineHeight > 1)
				Layout.LineSpacing = LineHeight;

			// GetDesiredSize(Allocation.Width, Allocation.Height, RequestMode == SizeRequestMode.HeightForWidth);
			return base.OnDrawn(cr);
		}

		private static Microsoft.Maui.Graphics.Platform.Gtk.TextLayout? _textLayout;

		public Microsoft.Maui.Graphics.Platform.Gtk.TextLayout SharedTextLayout => _textLayout ??= new Microsoft.Maui.Graphics.Platform.Gtk.TextLayout { HeightForWidth = true };

		public Size GetDesiredSize(double widthConstraint, double heightConstraint, bool heightForWidth)
		{

			var nativeView = this;
			int width = -1;
			int height = -1;

			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			var hMargin = nativeView.MarginStart + nativeView.MarginEnd;
			var vMargin = nativeView.MarginTop + nativeView.MarginBottom;

			SharedTextLayout.SetLayout(this.Layout);
			SharedTextLayout.FontDescription = nativeView.GetPangoFontDescription();

			SharedTextLayout.TextFlow = TextFlow.ClipBounds;
			SharedTextLayout.HorizontalAlignment = HorizontalTextAlignment.GetHorizontalAlignment();
			SharedTextLayout.VerticalAlignment = VerticalTextAlignment.GetVerticalAlignment();

			// SharedTextLayout.LineBreakMode = virtualView.LineBreakMode.GetLineBreakMode();

			var constraint = Math.Max(heightForWidth ? widthConstraint - hMargin : heightConstraint - vMargin,
				1);

			var lh = 0d;
			var layout = SharedTextLayout.GetLayout();
			layout.Height = -1;
			layout.Width = -1;
			layout.Ellipsize = nativeView.Ellipsize;
			layout.Spacing = nativeView.Layout.Spacing;

			layout.Attributes = nativeView.Attributes;

			if (LineHeight > 1)
				layout.LineSpacing = (float)LineHeight;
			else
			{
				layout.LineSpacing = 0;
			}

			layout.SetText(nativeView.Text);

			if (!heightConstrained)
			{
				if (nativeView.Lines > 0)
				{
					lh = layout.GetLineHeigth(nativeView.Lines, false);
					layout.Height = (int)lh;
				}
			}
			else
			{
				layout.Height = Math.Max((heightConstraint - vMargin).ScaledToPango(), -1);
			}

			if (!heightForWidth && heightConstrained && widthConstrained)
			{
				layout.Width = Math.Max((widthConstraint - hMargin).ScaledToPango(), -1);
			}

			(width, height) = layout.GetPixelSize(nativeView.Text, constraint, heightForWidth);

			if (lh > 0)
			{
				height = Math.Min((int)lh.ScaledFromPango(), height);
			}

			layout.Attributes = null;

			width += hMargin;
			height += vMargin;

			return new Size(width, height);

		}

	}

}