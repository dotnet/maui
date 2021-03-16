using Foundation;
using System;
using UIKit;

namespace Embedding.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			ShowWebView.TouchUpInside += (sender, e) => AppDelegate.Shared.ShowWebView();
			ShowAlertsAndActionSheets.TouchUpInside += (sender, e) => AppDelegate.Shared.ShowAlertsAndActionSheets();
		}
    }
}