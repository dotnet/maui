using CoreGraphics;
using UIKit;

namespace Microsoft.Maui
{
    public class NativeActivityIndicator : UIActivityIndicatorView
    {
        IActivityIndicator? _virtualView;

        public NativeActivityIndicator(CGRect rect, IActivityIndicator? virtualView) : base(rect)
            => _virtualView = virtualView;

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            if (_virtualView?.IsRunning == true)
                StartAnimating();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (_virtualView?.IsRunning == true)
                StartAnimating();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _virtualView = null;
        }
    }
}