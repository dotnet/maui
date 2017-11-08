using Gtk;
using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class StepperRenderer : ViewRenderer<Stepper, SpinButton>
    {
        private double _minimum;
        private double _maximum;

        protected override bool PreventGestureBubbling { get; set; } = true;
        
        protected override void Dispose(bool disposing)
        {
            if (Control != null)
                Control.ValueChanged -= OnValueChanged;

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    _minimum = 0;
                    _maximum = 100;

                    // Use Gtk.SpinButton. This Widget retrieve an integer or floating-point number from the user.
                    SetNativeControl(new SpinButton(_minimum, _maximum, 1));
                }

                // Detach change event until new element properties are initialized
                Control.ValueChanged -= OnValueChanged;

                UpdateMinimum();
                UpdateMaximum();
                UpdateValue();
                UpdateIncrement();

                Control.ValueChanged += OnValueChanged;
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
            else if (e.PropertyName == Stepper.BackgroundColorProperty.PropertyName)
                UpdateBackgroundColor();
        }

        protected override void UpdateBackgroundColor()
        {
            if (!Element.BackgroundColor.IsDefaultOrTransparent())
            {
                Control.ModifyBase(StateType.Normal, Element.BackgroundColor.ToGtkColor());
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            ElementController.SetValueFromRenderer(Stepper.ValueProperty, Control.Value);
        }

        private void UpdateIncrement()
        {
            var increment = Element.Increment;

            var decimalPart = increment - Math.Truncate(increment);

            if (decimalPart != 0)
            {
                var digits = decimalPart.ToString().Length - 2;
                Control.Digits = (uint)digits;
            }

            Control.SetIncrements(increment, 0);
        }

        private void UpdateMaximum()
        {
            _maximum = Element.Maximum;

            Control.SetRange(_minimum, _maximum);
        }

        private void UpdateMinimum()
        {
            _minimum = Element.Minimum;

            Control.SetRange(_minimum, _maximum);
        }

        private void UpdateValue()
        {
            if (Control.Value != Element.Value)
                Control.Value = Element.Value;
        }
    }
}