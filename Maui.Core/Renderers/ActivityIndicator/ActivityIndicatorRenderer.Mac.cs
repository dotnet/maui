using System.Linq;
using AppKit;
using CoreGraphics;
using CoreImage;

namespace System.Maui.Platform
{
	public partial class ActivityIndicatorRenderer : AbstractViewRenderer<IActivityIndicator, NSProgressIndicator>
	{
		CIColorPolynomial _currentColorFilter;
		NSColor _currentColor;

		protected override NSProgressIndicator CreateView()
		{
			return new NSProgressIndicator(CGRect.Empty) { Style = NSProgressIndicatorStyle.Spinning };
		}

		public static void MapPropertyIsRunning(IViewRenderer renderer, IActivityIndicator activityIndicator)
		{
			if (!(renderer.NativeView is NSProgressIndicator nSProgressIndicator))
				return;

			if (activityIndicator.IsRunning)
				nSProgressIndicator.StartAnimation(renderer.ContainerView);
			else
				nSProgressIndicator.StopAnimation(renderer.ContainerView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, IActivityIndicator activityIndicator)
		{
			if (!(renderer is ActivityIndicatorRenderer activityIndicatorRenderer) || !(renderer.NativeView is NSProgressIndicator nSProgressIndicator))
				return;

			var color = activityIndicator.Color;

			if (activityIndicatorRenderer._currentColorFilter == null && color.IsDefault)
				return;

			if (color.IsDefault)
				nSProgressIndicator.ContentFilters = new CIFilter[0];

			var newColor = activityIndicator.Color.ToNativeColor();

			if (Equals(activityIndicatorRenderer._currentColor, newColor))
			{
				if (nSProgressIndicator.ContentFilters?.FirstOrDefault() != activityIndicatorRenderer._currentColorFilter)
				{
					nSProgressIndicator.ContentFilters = new CIFilter[] { activityIndicatorRenderer._currentColorFilter };
				}
				return;
			}

			activityIndicatorRenderer._currentColor = newColor;

			activityIndicatorRenderer._currentColorFilter = new CIColorPolynomial
			{
				RedCoefficients = new CIVector(activityIndicatorRenderer._currentColor.RedComponent),
				BlueCoefficients = new CIVector(activityIndicatorRenderer._currentColor.BlueComponent),
				GreenCoefficients = new CIVector(activityIndicatorRenderer._currentColor.GreenComponent)
			};

			nSProgressIndicator.ContentFilters = new CIFilter[] { activityIndicatorRenderer._currentColorFilter };
		}
	}
}