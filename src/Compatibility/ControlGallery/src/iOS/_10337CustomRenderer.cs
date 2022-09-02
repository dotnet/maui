using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using ObjCRuntime;
using UIKit;

[assembly: ExportRenderer(typeof(Issue10337NavigationPage), typeof(_10337CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public class _10337CustomRenderer : Handlers.Compatibility.NavigationRenderer
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