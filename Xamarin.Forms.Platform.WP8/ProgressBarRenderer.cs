using System.ComponentModel;
using System.Windows;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, System.Windows.Controls.ProgressBar>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			var progressBar = new System.Windows.Controls.ProgressBar { Minimum = 0, Maximum = 1, Value = Element.Progress };
			progressBar.ValueChanged += ProgressBarOnValueChanged;

			SetNativeControl(progressBar);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Progress":
					Control.Value = Element.Progress;
					break;
			}
		}

		void ProgressBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged);
		}
	}
}