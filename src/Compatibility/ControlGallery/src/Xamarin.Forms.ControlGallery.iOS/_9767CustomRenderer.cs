using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Issue9767NavigationPage), typeof(_9767CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS
{
	public class _9767CustomRenderer : NavigationRenderer
    {
        public _9767CustomRenderer() : base()
        {
       
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UpdateColors();
            UpdateGradientView();
        }
		
		void UpdateColors()
		{
            UINavigationBar.Appearance.BarTintColor = UIColor.FromPatternImage(UIImage.FromFile("coffee.png"));
            UINavigationBar.Appearance.TintColor = UIColor.Yellow;
            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.Blue });
		}

        void UpdateGradientView()
        {
			var gradientLayer = new CAGradientLayer
			{
				Bounds = NavigationBar.Bounds,
				Colors = new CGColor[] { Color.Blue.ToCGColor(), Color.Purple.ToCGColor() },
				EndPoint = new CGPoint(0.0, 0.5),
				StartPoint = new CGPoint(1.0, 0.5)
			};
			UIGraphics.BeginImageContext(gradientLayer.Bounds.Size);
            gradientLayer.RenderInContext(UIGraphics.GetCurrentContext());
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            NavigationBar.SetBackgroundImage(image, UIBarMetrics.Default);
        }
    }
}