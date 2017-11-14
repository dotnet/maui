using Gtk;
using Xamarin.Forms.Platform.GTK.Packagers;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class LayoutRenderer : ViewRenderer<Layout, Fixed>
    {
        private Fixed _fixed;
        private LayoutElementPackager _packager;

        protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
        {
            if (e.OldElement != null)
            {
                e.OldElement.LayoutChanged -= LayoutChanged;
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    if(_fixed == null)
                    {
                        // Use a Gtk.Fixed, a container which you to position widgets at fixed coordinates.
                        // This allows apply transformations.
                        _fixed = new Fixed();
                    }

                    SetNativeControl(_fixed);
                }

                e.NewElement.LayoutChanged += LayoutChanged;

                if (_packager == null)
                {
                    _packager = new LayoutElementPackager(this);
                }

                _packager.Load();
            }

            base.OnElementChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Element != null)
                {
                    Element.LayoutChanged -= LayoutChanged;
                }
            }

            base.Dispose(disposing);
        }

        private void LayoutChanged(object sender, System.EventArgs e)
        {
            QueueResize();
        }
    }
}