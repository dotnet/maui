using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery.iOS.CustomRenderers;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using ObjCRuntime;
using UIKit;

[assembly: ExportRenderer(typeof(CustomFrame9974), typeof(_9774CustomRenderer))]
namespace Microsoft.Maui.Controls.ControlGallery.iOS.CustomRenderers
{
	public class _9774CustomRenderer : Handlers.Compatibility.FrameRenderer
	{
		public _9774CustomRenderer()
		{
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			Layer.ShadowRadius = 2.0f;
			Layer.ShadowOffset = new CGSize(2, 2);
			Layer.ShadowOpacity = 0.4f;
			Layer.ShadowPath = UIBezierPath.FromRoundedRect(Layer.Bounds, 15).CGPath;
			Layer.MasksToBounds = false;
			Layer.CornerRadius = 15;
			Layer.BorderColor = UIColor.FromRGB(211, 211, 211).CGColor;
			Layer.BackgroundColor = UIColor.White.CGColor;
			Layer.BorderWidth = 1;
		}
	}
}