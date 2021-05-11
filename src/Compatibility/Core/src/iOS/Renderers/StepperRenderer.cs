using System;
using System.ComponentModel;
using System.Drawing;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class StepperRenderer : ViewRenderer<Stepper, UIStepper>
	{
		bool _disposed;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public StepperRenderer()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Control != null)
					Control.ValueChanged -= OnValueChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new UIStepper(RectangleF.Empty));
					Control.ValueChanged += OnValueChanged;
				}

				UpdateMinimum();
				UpdateMaximum();
				UpdateValue();
				UpdateIncrement();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Stepper.MinimumProperty.PropertyName)
				UpdateMinimum();
			else if (e.PropertyName == Stepper.MaximumProperty.PropertyName)
				UpdateMaximum();
			else if (e.PropertyName == Stepper.ValueProperty.PropertyName)
				UpdateValue();
			else if (e.PropertyName == Stepper.IncrementProperty.PropertyName)
				UpdateIncrement();
		}

		[PortHandler]
		void OnValueChanged(object sender, EventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(Stepper.ValueProperty, Control.Value);
		}

		[PortHandler]
		void UpdateIncrement()
		{
			Control.StepValue = Element.Increment;
		}

		[PortHandler]
		void UpdateMaximum()
		{
			Control.MaximumValue = Element.Maximum;
		}

		[PortHandler]
		void UpdateMinimum()
		{
			Control.MinimumValue = Element.Minimum;
		}

		[PortHandler]
		void UpdateValue()
		{
			if (Control.Value != Element.Value)
				Control.Value = Element.Value;
		}
	}
}