
using System;
using System.ComponentModel;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Diagnostics;
using System.Maui.Core;

#if __MOBILE__
using NativeLabel = UIKit.UILabel;
#else
using NativeLabel = AppKit.NSTextField;
#endif


namespace System.Maui.Platform
{
	public partial class LabelRenderer : AbstractViewRenderer<ILabel, NativeLabel>
	{
		//static Color? DefaultTextColor;

		protected override NativeLabel CreateView()
		{
			var label = new NativeLabel();
			//if (DefaultTextColor == null)
			//	DefaultTextColor = label.TextColor.ToColor();
			return label;
		}


		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var result = base.GetDesiredSize(widthConstraint, heightConstraint);
			var tinyWidth = Math.Min(10, result.Request.Width);
			result.Minimum = new Size(tinyWidth, result.Request.Height);

			return result;
		}


		public static void MapPropertyText(IViewRenderer renderer, IText view)
		{
			var label = renderer.NativeView as NativeLabel;
			if(label == null)
				return;

			if (view.TextType == TextType.Html)
			{

				label.SetText(view.Text.ToNSAttributedString());
			}
			else
			{
				label.SetText(view.Text);
			}

			view.InvalidateMeasure();
#if __MOBILE__
			label.Superview?.SetNeedsLayout();
#endif
		}
		public static void MapPropertyColor (IViewRenderer renderer, IText view) {
			var label = renderer.NativeView as NativeLabel;
			if (label == null)
				return;
			label.TextColor = view.Color.ToNativeColor();
		}


		public static void MapPropertyLineHeight(IViewRenderer renderer, ILabel view)
		{

		}
	}
}
