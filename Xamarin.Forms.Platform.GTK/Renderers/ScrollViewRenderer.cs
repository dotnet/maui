using Gtk;
using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class ScrollViewRenderer : ViewRenderer<ScrollView, ScrolledWindow>
    {
        private VisualElement _currentView;
        private Viewport _viewPort;

        protected IScrollViewController Controller
        {
            get { return Element; }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.OldElement.ScrollToRequested -= OnScrollToRequested;
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    // Use Gtk.ScrolledWindow that adds scrollbars to its child widget.
                    Control = new ScrolledWindow
                    {
                        CanFocus = true,
                        ShadowType = ShadowType.None,
                        BorderWidth = 0,
                        HscrollbarPolicy = PolicyType.Automatic,
                        VscrollbarPolicy = PolicyType.Automatic
                    };

                    _viewPort = new Viewport();
                    _viewPort.ShadowType = ShadowType.None;
                    _viewPort.BorderWidth = 0;

                    Control.Add(_viewPort);
                    SetNativeControl(Control);

                    Control.Hadjustment.ValueChanged += OnScrollEvent;
                    Control.Vadjustment.ValueChanged += OnScrollEvent;
                }

                Element.ScrollToRequested += OnScrollToRequested;

                UpdateOrientation();
                LoadContent();
                UpdateContentSize();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName)
                UpdateContentSize();
            else if (e.PropertyName == nameof(ScrollView.Content))
                LoadContent();
            else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
                UpdateOrientation();
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                Control.ScrollEvent -= OnScrollEvent;

                if (Control.Hadjustment != null)
                    Control.Hadjustment.ValueChanged -= OnScrollEvent;

                if (Control.Vadjustment != null)
                    Control.Vadjustment.ValueChanged -= OnScrollEvent;
            }

            base.Dispose(disposing);
        }

        protected override void UpdateBackgroundColor()
        {
            if (Element.BackgroundColor.IsDefaultOrTransparent())
            {
                return;
            }

            var backgroundColor = Element.BackgroundColor;

            if (Control != null)
            {
                Control.ModifyBg(StateType.Normal, backgroundColor.ToGtkColor());
            }

            if (_viewPort != null)
            {
                _viewPort.ModifyBg(StateType.Normal, backgroundColor.ToGtkColor());
            }

            base.UpdateBackgroundColor();
        }

        private void OnScrollEvent(object o, EventArgs args)
        {
            Controller.SetScrolledPosition(Control.Hadjustment.Value, Control.Vadjustment.Value);
        }

        private void LoadContent()
        {
            if (_currentView != null)
            {
                _currentView.Cleanup();
            }

            _currentView = Element.Content;

            IVisualElementRenderer renderer = null;

            if (_currentView != null)
            {
                renderer = _currentView.GetOrCreateRenderer();
                renderer.Container.Unparent();
            }

            if (renderer != null)
            {
                var content = renderer.Container;
                content.VisibleWindow = false;
                _viewPort.Add(content);
            }
        }

        private void UpdateOrientation()
        {
            if (Control == null)
                return;

            switch (Element.Orientation)
            {
                case ScrollOrientation.Vertical:
                    Control.HscrollbarPolicy = PolicyType.Never;
                    Control.VscrollbarPolicy = PolicyType.Automatic;
                    break;
                case ScrollOrientation.Horizontal:
                    Control.HscrollbarPolicy = PolicyType.Automatic;
                    Control.VscrollbarPolicy = PolicyType.Never;
                    break;
                case ScrollOrientation.Both:
                    Control.HscrollbarPolicy = PolicyType.Automatic;
                    Control.VscrollbarPolicy = PolicyType.Automatic;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Element.Orientation));
            }
        }

        private void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
        {
            double x = e.ScrollX, y = e.ScrollY;

            ScrollToMode mode = e.Mode;

            if (mode == ScrollToMode.Element)
            {
                Point pos = Controller.GetScrollPositionForElement((VisualElement)e.Element, e.Position);
                x = pos.X;
                y = pos.Y;
                mode = ScrollToMode.Position;
            }

            if (mode == ScrollToMode.Position)
            {
                Control.Hadjustment.Value = x;
                Control.Vadjustment.Value = y;
            }

            Controller.SendScrollFinished();
        }

        private void UpdateContentSize()
        {
            if (Control == null)
                return;

            var contentSize = Element.ContentSize;

            var height = Convert.ToInt32(contentSize.Height);
            var width = Convert.ToInt32(contentSize.Width);

            Control.SetSizeRequest(width, height);
        }
    }
}