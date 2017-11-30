using System.ComponentModel;
using Windows.UI.Xaml.Controls.Primitives;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, Windows.UI.Xaml.Controls.ProgressBar>
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.ValueChanged -= ProgressBarOnValueChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var progressBar = new Windows.UI.Xaml.Controls.ProgressBar { Minimum = 0, Maximum = 1 };

					progressBar.ValueChanged += ProgressBarOnValueChanged;

					SetNativeControl(progressBar);
				}

				Control.Value = e.NewElement.Progress;
				UpdateFlowDirection();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				Control.Value = Element.Progress;
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void ProgressBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}
	}
}