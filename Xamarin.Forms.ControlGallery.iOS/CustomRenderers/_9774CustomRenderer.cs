using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.CustomRenderers;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomFrame9974), typeof(_9774CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS.CustomRenderers
{
	public class _9774CustomRenderer : FrameRenderer
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