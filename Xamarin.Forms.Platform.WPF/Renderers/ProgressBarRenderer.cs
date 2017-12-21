using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
					SetNativeControl(new WProgressBar { Minimum = 0, Maximum = 1, Foreground = Brushes.DeepSkyBlue });
					Control.ValueChanged += HandleValueChanged;
				}

				// Update control property 
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