using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery.iOS.CustomRenderers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.ControlGallery.Issues.Issue6368;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(RoundedLabel), typeof(RoundedLabelRenderer))]
#pragma warning restore CS0612 // Type or member is obsolete
namespace Microsoft.Maui.Controls.ControlGallery.iOS.CustomRenderers
{
	[System.Obsolete]
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
				this.Layer.BorderColor = UIColor.FromRGB(3, 169, 244).CGColor;
				this.Layer.BackgroundColor = Colors.GhostWhite.ToCGColor();
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
