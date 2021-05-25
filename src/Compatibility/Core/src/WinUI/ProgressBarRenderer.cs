using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, Microsoft.UI.Xaml.Controls.ProgressBar>
	{
		object _foregroundDefault;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.ValueChanged -= ProgressBarOnValueChanged;
					Control.Loaded -= OnControlLoaded;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				if (Control != null)
				{
					Control.Loaded -= OnControlLoaded;
				}
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var progressBar = new Microsoft.UI.Xaml.Controls.ProgressBar { Minimum = 0, Maximum = 1 };

					progressBar.ValueChanged += ProgressBarOnValueChanged;

					SetNativeControl(progressBar);

					Control.Loaded += OnControlLoaded;
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
			else if (e.PropertyName == ProgressBar.ProgressColorProperty.PropertyName)
				UpdateProgressColor();
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_foregroundDefault = Control.GetForegroundCache();
			UpdateProgressColor();
		}

		void UpdateProgressColor()
		{
			Color color = Element.ProgressColor;

			if (color.IsDefault())
			{
				Control.RestoreForegroundCache(_foregroundDefault);
			}
			else
			{
				Control.Foreground = Maui.ColorExtensions.ToNative(color);
			}
		}

		[PortHandler]
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