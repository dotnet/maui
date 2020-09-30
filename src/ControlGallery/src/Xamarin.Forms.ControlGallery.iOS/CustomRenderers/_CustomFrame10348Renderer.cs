using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.CustomRenderers;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomFrame10348), typeof(_CustomFrame10348Renderer))]
namespace Xamarin.Forms.ControlGallery.iOS.CustomRenderers
{
	public class _CustomFrame10348Renderer : FrameRenderer
	{
        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            Layer.ShadowRadius = 10f;
            Layer.ShadowColor = UIColor.Blue.CGColor;
            Layer.ShadowOffset = new CGSize(0, 10);
            Layer.ShadowOpacity = 0.16f;
        }
    }
}