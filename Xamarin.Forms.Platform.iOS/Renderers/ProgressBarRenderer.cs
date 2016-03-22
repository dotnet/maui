using System.Drawing;
using System.ComponentModel;
#if __UNIFIED__
using UIKit;
#else
using MonoTouch.UIKit;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

#else
using nfloat=System.Single;
using nint=System.Int32;
using nuint=System.UInt32;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, UIProgressView>
	{
		public override SizeF SizeThatFits(SizeF size)
		{
			// progress bar will size itself to be as wide as the request, even if its inifinite
			// we want the minimum need size
			var result = base.SizeThatFits(size);
			return new SizeF(10, result.Height);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new UIProgressView(UIProgressViewStyle.Default));

				UpdateProgress();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				UpdateProgress();
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;

			Control.TrackTintColor = color != Color.Default ? color.ToUIColor() : null;
		}

		void UpdateProgress()
		{
			Control.Progress = (float)Element.Progress;
		}
	}
}