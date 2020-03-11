using System.ComponentModel;
using System.Drawing;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, UIActivityIndicatorView>
	{
		[Internals.Preserve(Conditional = true)]
		public ActivityIndicatorRenderer()
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
#if __XCODE11__
					if(Forms.IsiOS13OrNewer)
						SetNativeControl(new UIActivityIndicatorView(RectangleF.Empty) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Medium });
					else
#endif
						SetNativeControl(new UIActivityIndicatorView(RectangleF.Empty) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray });
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

		void UpdateColor()
		{
			Control.Color = Element.Color == Color.Default ? null : Element.Color.ToUIColor();
		}

		void UpdateIsRunning()
		{
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
