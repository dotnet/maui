using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class SwitchRenderer : ViewRenderer<Switch, Gtk.CheckButton>
    {
        private bool _disposed;

        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            if (e.OldElement != null)
                e.OldElement.Toggled -= OnElementToggled;

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                   // Use Gtk.CheckButton, a discrete toggle button.
                    SetNativeControl(new Gtk.CheckButton());
                }

                Control.Toggled -= OnCheckButtonToggled;

                UpdateState();
                UpdateBackgroundColor();
                e.NewElement.Toggled += OnElementToggled;

                Control.Toggled += OnCheckButtonToggled;
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
            {
                Control.Active = Element.IsToggled;
            }
            else if (e.PropertyName == Switch.BackgroundColorProperty.PropertyName)
            {
                UpdateBackgroundColor();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                if (Control != null)
                {
                    Control.Toggled -= OnCheckButtonToggled;
                }
            }

            base.Dispose(disposing);
        }

        private void OnElementToggled(object sender, EventArgs e)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            Control.Active = Element.IsToggled ? true : false;
        }

        private void OnCheckButtonToggled(object sender, EventArgs e)
        {
            ElementController.SetValueFromRenderer(Switch.IsToggledProperty, Control.Active);
        }
    }
}