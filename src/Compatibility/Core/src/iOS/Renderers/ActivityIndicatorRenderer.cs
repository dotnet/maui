using CoreGraphics;
using System.ComponentModel;
using System.Drawing;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[PortHandler]
	public sealed class UIActivityIndicatorViewDelegate : UIActivityIndicatorView
    {
        ActivityIndicator _element;
        public UIActivityIndicatorViewDelegate(RectangleF point, ActivityIndicator element) : base(point)
            => _element = element;

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            if (_element?.IsRunning == true)
                StartAnimating();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (_element?.IsRunning == true)
                StartAnimating();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _element = null;
        }
    }
	
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, UIActivityIndicatorView>
	{
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public ActivityIndicatorRenderer()
		{

		}

		[PortHandler]
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					if(Forms.IsiOS13OrNewer)
						SetNativeControl(new UIActivityIndicatorViewDelegate(RectangleF.Empty, e.NewElement) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Medium });
					else
						SetNativeControl(new UIActivityIndicatorViewDelegate(RectangleF.Empty, e.NewElement) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray });
				}

				UpdateColor();
				UpdateIsRunning();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
				UpdateIsRunning();
		}

		[PortHandler]
		void UpdateColor()
		{
			Control.Color = Element.Color == Color.Default ? null : Element.Color.ToUIColor();
		}

		[PortHandler]
		void UpdateIsRunning()
		{
			if (Control?.Superview == null)
                return;
				
			if (Element.IsRunning)
				Control.StartAnimating();
			else
				Control.StopAnimating();
		}

		internal void PreserveState()
		{
			if (Control != null && !Control.IsAnimating && Element != null && Element.IsRunning)
				Control.StartAnimating();
		}
	}
}
