using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class StepperRenderer : ViewRenderer<Stepper, NSStepper>
	{
		bool _disposed;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new NSStepper());
					Control.Activated += OnControlActivated;
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

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
					Control.Activated -= OnControlActivated;
			}

			base.Dispose(disposing);
		}

		void OnControlActivated(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(Stepper.ValueProperty, Control.DoubleValue);
		}

		void UpdateIncrement()
		{
			Control.Increment = Element.Increment;
		}

		void UpdateMaximum()
		{
			Control.MaxValue = Element.Maximum;
		}

		void UpdateMinimum()
		{
			Control.MinValue = Element.Minimum;
		}

		void UpdateValue()
		{
			if (Math.Abs(Control.DoubleValue - Element.Value) > 0)
				Control.DoubleValue = Element.Value;
		}
	}
}