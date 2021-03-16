using CoreGraphics;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomRenderers;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomFrame10348), typeof(_CustomFrame10348Renderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomRenderers
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