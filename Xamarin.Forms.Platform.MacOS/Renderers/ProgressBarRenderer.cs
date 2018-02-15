using System.ComponentModel;
using AppKit;
using CoreImage;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, NSProgressIndicator>
	{
		static CIColorPolynomial s_currentColorFilter;
		static NSColor s_currentColor;

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (e.NewElement == null) return;
			if (Control == null)
				SetNativeControl(new NSProgressIndicator
				{
					IsDisplayedWhenStopped = true,
					Indeterminate = false,
					Style = NSProgressIndicatorStyle.Bar,
					MinValue = 0,
					MaxValue = 1
				});
			UpdateProgressColor();
			UpdateProgress();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ProgressBar.ProgressColorProperty.PropertyName)
				UpdateProgressColor();
			else if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				UpdateProgress();
		}

		void UpdateProgressColor()
		{
			SetBackgroundColor(Element.ProgressColor);
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Control == null)
				return;

			if (color.IsDefault)
			{
				if (s_currentColorFilter != null && Element.BackgroundColor.IsDefault && Element.ProgressColor.IsDefault)
				{
					Control.ContentFilters = new CIFilter[0];
					s_currentColor = null;
				}

				return;
			}

			var newColor = color.ToNSColor();
			if (Equals(s_currentColor, newColor))
				return;

			s_currentColor = newColor;

			s_currentColorFilter = new CIColorPolynomial
			{
				RedCoefficients = new CIVector(s_currentColor.RedComponent),
				BlueCoefficients = new CIVector(s_currentColor.BlueComponent),
				GreenCoefficients = new CIVector(s_currentColor.GreenComponent)
			};

			Control.ContentFilters = new CIFilter[] { s_currentColorFilter };
		}

		void UpdateProgress()
		{
			Control.DoubleValue = Element.Progress;
		}
	}
}