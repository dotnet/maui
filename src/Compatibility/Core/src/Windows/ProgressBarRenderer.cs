using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using WBinding = Microsoft.UI.Xaml.Data.Binding;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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
			_foregroundDefault = GetForegroundCache();
			UpdateProgressColor();
		}

		[PortHandler]
		void UpdateProgressColor()
		{
			Color color = Element.ProgressColor;

			if (color.IsDefault())
			{
				RestoreForegroundCache();
			}
			else
			{
				Control.Foreground = color.ToPlatform();
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

		private object GetForegroundCache()
		{
			return Control.GetBindingExpression(Microsoft.UI.Xaml.Controls.Control.ForegroundProperty)?.ParentBinding
				?? Control.GetValue(Microsoft.UI.Xaml.Controls.Control.ForegroundProperty);
		}

		private void RestoreForegroundCache()
		{
			if (_foregroundDefault is WBinding binding)
			{
				Control.SetBinding(Microsoft.UI.Xaml.Controls.Control.ForegroundProperty, binding);
			}
			else
			{
				Control.SetValue(Microsoft.UI.Xaml.Controls.Control.ForegroundProperty, (WBrush)_foregroundDefault);
			}
		}
	}
}