using NativeView = Gtk.Widget;

namespace Xamarin.Forms.Platform.GTK
{
    public abstract class ViewRenderer : ViewRenderer<View, NativeView>
    {

    }

    public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView, TNativeView>
        where TView : View where TNativeView : NativeView
    {
        private string _defaultAccessibilityLabel;
        private string _defaultAccessibilityHint;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Control != null)
            {
                Control.Dispose();
                Control = null;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                UpdateBackgroundColor();
            }
        }

        protected override void SetNativeControl(TNativeView view)
        {
            base.SetNativeControl(view);

            Add(view);
        }

        protected override void SetAccessibilityHint()
        {
            if (Control == null)
            {
                base.SetAccessibilityHint();
                return;
            }

            if (Element == null)
                return;

            if (_defaultAccessibilityHint == null)
                _defaultAccessibilityHint = Control.Accessible.Name;

            var helpText = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;

            if (!string.IsNullOrEmpty(helpText))
            {
                Control.Accessible.Name = helpText;
            }
        }

        protected override void SetAccessibilityLabel()
        {
            if (Control == null)
            {
                base.SetAccessibilityLabel();
                return;
            }

            if (Element == null)
                return;

            if (_defaultAccessibilityLabel == null)
                _defaultAccessibilityLabel = Control.Accessible.Description;

            var name = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;

            if (!string.IsNullOrEmpty(name))
            {
                Control.Accessible.Description = name;
            }
        }
    }
}