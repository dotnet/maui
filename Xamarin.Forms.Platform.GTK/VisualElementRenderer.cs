using Gtk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;
using Container = Gtk.EventBox;
using Control = Gtk.Widget;

namespace Xamarin.Forms.Platform.GTK
{
    public class VisualElementRenderer<TElement, TNativeElement> : Container, IVisualElementRenderer, IDisposable, IEffectControlProvider
        where TElement : VisualElement
        where TNativeElement : Control
    {
        private bool _disposed;
        private readonly PropertyChangedEventHandler _propertyChangedHandler;
        private readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();
        private VisualElementTracker<TElement, TNativeElement> _tracker;
        private string _defaultAccessibilityLabel;
        private string _defaultAccessibilityHint;

        protected VisualElementRenderer()
        {
            _propertyChangedHandler = OnElementPropertyChanged;
        }

        protected VisualElementTracker<TElement, TNativeElement> Tracker
        {
            get { return _tracker; }
            set
            {
                if (_tracker == value)
                    return;

                if (_tracker != null)
                {
                    _tracker.Dispose();
                    _tracker.Updated -= OnTrackerUpdated;
                }

                _tracker = value;

                if (_tracker != null)
                {
                    _tracker.Updated += OnTrackerUpdated;
                    UpdateTracker();
                }
            }
        }

        public TNativeElement Control { get; set; }

        public TElement Element { get; set; }

        public Container Container => this;

        public bool Disposed { get { return _disposed; } }

        VisualElement IVisualElementRenderer.Element
        {
            get
            {
                return Element;
            }
        }

        protected IElementController ElementController => Element as IElementController;

        protected virtual bool PreventGestureBubbling { get; set; } = false;

        event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
        {
            add { _elementChangedHandlers.Add(value); }
            remove { _elementChangedHandlers.Remove(value); }
        }

        public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

        void IEffectControlProvider.RegisterEffect(Effect effect)
        {
            var platformEffect = effect as PlatformEffect;
            if (platformEffect != null)
                OnRegisterEffect(platformEffect);
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            SetElement((TElement)element);
        }

        public void SetElement(TElement element)
        {
            var oldElement = Element;
            Element = element;

            if (oldElement != null)
            {
                oldElement.FocusChangeRequested -= OnElementFocusChangeRequested;
                oldElement.PropertyChanged -= _propertyChangedHandler;
            }

            if (element != null)
            {
                element.PropertyChanged += _propertyChangedHandler;
                element.FocusChangeRequested += OnElementFocusChangeRequested;

                if (Tracker == null)
                {
                    Tracker = new VisualElementTracker<TElement, TNativeElement>();
                }
            }

            OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, element));

            SetAccessibilityLabel();
            SetAccessibilityHint();
        }

        public void SetElementSize(Size size)
        {
            Layout.LayoutChildIntoBoundingRegion(Element,
                new Rectangle(Element.X, Element.Y, size.Width, size.Height));
        }

        public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (Children.Length == 0)
                return new SizeRequest();

            return Control.GetDesiredSize(widthConstraint, heightConstraint);
        }

        protected override void OnShown()
        {
            base.OnShown();

            UpdateIsVisible();
        }

        public override void Dispose()
        {
            base.Dispose();

            Dispose(true);
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            double width, height, translationX, translationY;
            Rectangle bounds = Element.Bounds;

            translationX = Element.TranslationX;
            translationY = Element.TranslationY;

            width = bounds.Width >= -1 ? bounds.Width : 0;
            height = bounds.Height >= -1 ? bounds.Height : 0;

            Container.SetSize(width, height);
            Container.MoveTo((int)bounds.X + translationX, (int)bounds.Y + translationY);

            for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
            {
                var child = ElementController.LogicalChildren[i] as VisualElement;

                if (child != null)
                {
                    var renderer = Platform.GetRenderer(child);

                    if (renderer != null)
                    {
                        width = child.Bounds.Width >= -1 ? child.Bounds.Width : 0;
                        height = child.Bounds.Height >= -1 ? child.Bounds.Height : 0;

                        translationX = child.TranslationX;
                        translationY = child.TranslationY;

                        renderer.Container.SetSize(width, height);
                        renderer.Container.MoveTo(child.Bounds.X + translationX, child.Bounds.Y + translationY);
                    }
                }
            }
        }

        protected virtual void OnRegisterEffect(PlatformEffect effect)
        {
            effect.SetContainer(this);
            effect.SetControl(Container);
        }

        protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
        {
            var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
            for (var i = 0; i < _elementChangedHandlers.Count; i++)
                _elementChangedHandlers[i](this, args);

            ElementChanged?.Invoke(this, e);
        }

        protected virtual void SetNativeControl(TNativeElement view)
        {
            Control = view;

            UpdateBackgroundColor();
            UpdateIsVisible();
            UpdateSensitive();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
                return;

            _disposed = true;

            Tracker?.Dispose();
            Tracker = null;
        }

        protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
                UpdateIsVisible();
            else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
                UpdateBackgroundColor();
            else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
                UpdateSensitive();
            else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
                SetAccessibilityHint();
            else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
                SetAccessibilityLabel();
        }

        protected virtual void UpdateBackgroundColor()
        {
            if (_disposed || Element == null || Control == null)
                return;

            Color backgroundColor = Element.BackgroundColor;

            bool isDefault = backgroundColor.IsDefaultOrTransparent();

            if (!isDefault)
            {
                Container.ModifyBg(StateType.Normal, backgroundColor.ToGtkColor());
            }

            Container.VisibleWindow = !isDefault;
        }

        protected virtual void SetAccessibilityHint()
        {
            if (_disposed || Element == null || Control == null)
                return;

            if (_defaultAccessibilityHint == null)
                _defaultAccessibilityHint = Accessible.Name;

            var helpText = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;

            if (!string.IsNullOrEmpty(helpText))
            {
                Accessible.Name = helpText;
            }
        }

        protected virtual void SetAccessibilityLabel()
        {
            if (_disposed || Element == null || Control == null)
                return;

            if (_defaultAccessibilityLabel == null)
                _defaultAccessibilityLabel = Accessible.Description;

            var name = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;

            if (!string.IsNullOrEmpty(name))
            {
                Accessible.Description = name;
            }
        }

        protected virtual void UpdateNativeControl()
        {
            UpdateSensitive();
        }

        internal virtual void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
        {
            var control = Control as Control;

            if (control == null)
                return;

            if (args.Focus)
                args.Result = control.IsFocus = true;
            else
            {
                control.IsFocus = false;
                args.Result = true;
            }
        }

        private void UpdateIsVisible()
        {
            if (_disposed || Element == null || Control == null)
                return;

            Container.Visible = Element.IsVisible;
        }

        private void UpdateSensitive()
        {
            if (_disposed || Element == null || Control == null)
                return;

            Control.Sensitive = Element.IsEnabled;
        }

        private void OnTrackerUpdated(object sender, EventArgs e)
        {
            UpdateNativeControl();
        }

        private void UpdateTracker()
        {
            if (_tracker == null)
                return;

            _tracker.PreventGestureBubbling = PreventGestureBubbling;
            _tracker.Control = Control;
            _tracker.Element = Element;
            _tracker.Container = Container;
        }
    }
}