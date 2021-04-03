using System;
using UIKit;

namespace GraphicsTester.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
            View = new TesterView(View.Bounds);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            var frame = View.Window.Frame;
            View.Frame = frame;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }
    }
}
