using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls.Issues;
using System.Maui.Platform.iOS;

[assembly: ExportRenderer(typeof(Issue10337NavigationPage), typeof(_10337CustomRenderer))]
namespace System.Maui.ControlGallery.iOS
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