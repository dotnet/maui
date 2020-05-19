using System;
using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS.CustomRenderers;
using System.Maui.Platform.iOS;
using static System.Maui.Controls.Issues.Issue6368;

[assembly: ExportRenderer(typeof(RoundedLabel), typeof(RoundedLabelRenderer))]
namespace System.Maui.ControlGallery.iOS.CustomRenderers
{
	public class RoundedLabelRenderer : LabelRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			if (Control == null)
			{
				SetNativeControl(new TagUiLabel());
			}
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				this.Layer.CornerRadius = 10;
				this.Layer.BorderColor = ColorExtensions.ToCGColor(ColorExtensions.ToColor(UIColor.FromRGB(3, 169, 244)));
				this.Layer.BackgroundColor = ColorExtensions.ToCGColor(Color.GhostWhite);
				this.Layer.BorderWidth = 1;
			}
		}
	}

	class TagUiLabel : UILabel
	{
		UIEdgeInsets _edgeInsets = new UIEdgeInsets(10, 5, 10, 5);
		UIEdgeInsets _inverseEdgeInsets = new UIEdgeInsets(-10, -5, -10, -5);

		public override CoreGraphics.CGRect TextRectForBounds(CoreGraphics.CGRect bounds, nint numberOfLines)
		{
			var textRect = base.TextRectForBounds(_edgeInsets.InsetRect(bounds), numberOfLines);
			return _inverseEdgeInsets.InsetRect(textRect);
		}
		public override void DrawText(CoreGraphics.CGRect rect)
		{
			base.DrawText(_edgeInsets.InsetRect(rect));
		}
	}
}
