using System.Linq;
using AppKit;
using CoreImage;

namespace System.Maui.Platform
{
	public partial class ProgressBarRenderer : AbstractViewRenderer<IProgress, NSProgressIndicator>
	{
		static CIColorPolynomial _currentColorFilter;
		static NSColor _currentColor;

		protected override NSProgressIndicator CreateView()
		{
			return new NSProgressIndicator
			{
				IsDisplayedWhenStopped = true,
				Indeterminate = false,
				Style = NSProgressIndicatorStyle.Bar,
				MinValue = 0,
				MaxValue = 1
			};
		}

		public static void MapPropertyProgress(IViewRenderer renderer, IProgress progressBar)
		{
			if (!(renderer.NativeView is NSProgressIndicator nSProgressIndicator))
				return;

			nSProgressIndicator.DoubleValue = progressBar.Progress;
		}

		public static void MapPropertyProgressColor(IViewRenderer renderer, IProgress progressBar)
		{
			if (!(renderer.NativeView is NSProgressIndicator nSProgressIndicator))
				return;

			var progressColor = progressBar.ProgressColor;

			if (progressColor.IsDefault)
				return;
			
			var newProgressColor = progressColor.ToNativeColor();

			if (Equals(_currentColor, newProgressColor))
			{
				if (nSProgressIndicator.ContentFilters?.FirstOrDefault() != _currentColorFilter)
				{
					nSProgressIndicator.ContentFilters = new CIFilter[] { _currentColorFilter };
				}
				return;
			}

			_currentColor = newProgressColor;

			_currentColorFilter = new CIColorPolynomial
			{
				RedCoefficients = new CIVector(_currentColor.RedComponent),
				BlueCoefficients = new CIVector(_currentColor.BlueComponent),
				GreenCoefficients = new CIVector(_currentColor.GreenComponent)
			};

			nSProgressIndicator.ContentFilters = new CIFilter[] { _currentColorFilter };
		}
	}
}