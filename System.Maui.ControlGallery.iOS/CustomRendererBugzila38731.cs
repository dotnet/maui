using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls.Issues;

[assembly: ExportRenderer(typeof(Bugzilla38731), typeof(CustomRendererBugzila38731))]
[assembly: ExportRenderer(typeof(Bugzilla38731.PageTwo), typeof(CustomRendererBugzila38731))]
[assembly: ExportRenderer(typeof(Bugzilla38731.PageThree), typeof(CustomRendererBugzila38731))]
[assembly: ExportRenderer(typeof(Bugzilla38731.PageFour), typeof(CustomRendererBugzila38731))]

namespace System.Maui.ControlGallery.iOS
{
	public class CustomRendererBugzila38731 : Platform.iOS.PageRenderer
	{
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			if (NavigationController.ViewControllers.Length > 1)
			{
				var btn = new UIBarButtonItem(UIImage.FromFile("bank.png"), UIBarButtonItemStyle.Plain, (sender, args) =>
					{
						NavigationController.PopViewController(true);
					});
				btn.AccessibilityIdentifier = "goback";
				NavigationController.TopViewController.NavigationItem.SetLeftBarButtonItem(btn, true);
			}
		}
	}
}
