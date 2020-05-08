using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Issue10337NavigationPage), typeof(_10337CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS
{
    public class _10337CustomRenderer : NavigationRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationBar.ShadowImage = new UIImage();
            UINavigationBar.Appearance.BackIndicatorImage = new UIImage();
            UINavigationBar.Appearance.BackIndicatorTransitionMaskImage = new UIImage();
        }
    }
}