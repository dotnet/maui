using System.Drawing;
using System.ComponentModel;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, UIActivityIndicatorView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
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
	}
}