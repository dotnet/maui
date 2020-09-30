using System.ComponentModel;
using System.Windows;
using Xamarin.Forms.Internals;
using WProgressBar = System.Windows.Controls.ProgressBar;

namespace Xamarin.Forms.Platform.WPF
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, WProgressBar>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new WProgressBar { Minimum = 0, Maximum = 1 });
					Control.ValueChanged += HandleValueChanged;
				}

				// Update control property 
				UpdateProgress();
				UpdateProgressColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				UpdateProgress();
			else if (e.PropertyName == ProgressBar.ProgressColorProperty.PropertyName)
				UpdateProgressColor();
		}

		void UpdateProgressColor()
		{
			Control.UpdateDependencyColor(WProgressBar.ForegroundProperty, Element.ProgressColor.IsDefault ? Color.DeepSkyBlue : Element.ProgressColor);
		}

		void UpdateProgress()
		{
			Control.Value = Element.Progress;
		}

		void HandleValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.ValueChanged -= HandleValueChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}