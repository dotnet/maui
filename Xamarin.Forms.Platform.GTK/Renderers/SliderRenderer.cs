using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class SliderRenderer : ViewRenderer<Slider, Gtk.HScale>
    {
        private double _minimum;
        private double _maximum;
        private bool _disposed;

        protected override bool PreventGestureBubbling { get; set; } = true;

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
                Control.ValueChanged -= OnControlValueChanged;

            if (disposing && !_disposed)
            {
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    _minimum = e.NewElement.Minimum;
                    _maximum = e.NewElement.Maximum;
                    double stepping = Math.Min((e.NewElement.Maximum - e.NewElement.Minimum) / 10, 1);

                    // Use gtk.HScale, a horizontal slider widget for selecting a value from a range.
                    SetNativeControl(new Gtk.HScale(_minimum, _maximum, stepping));
                    Control.ValueChanged += OnControlValueChanged;
                }

                UpdateMaximum();
                UpdateMinimum();
                UpdateValue();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Slider.MaximumProperty.PropertyName)
                UpdateMaximum();
            else if (e.PropertyName == Slider.MinimumProperty.PropertyName)
                UpdateMinimum();
            else if (e.PropertyName == Slider.ValueProperty.PropertyName)
                UpdateValue();
        }

        private void OnControlValueChanged(object sender, EventArgs eventArgs)
        {
            ElementController.SetValueFromRenderer(Slider.ValueProperty, Control.Value);
        }

        private void UpdateMaximum()
        {
            _maximum = (float)Element.Maximum;

            Control.SetRange(_minimum, _maximum);
        }

        private void UpdateMinimum()
        {
            _minimum = (float)Element.Minimum;

            Control.SetRange(_minimum, _maximum);
        }

        private void UpdateValue()
        {
            Control.Value = (float)Element.Value;
        }
    }
}