using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class NavigationChildPage : Gtk.Object
    {
        bool _disposed;

        public NavigationChildPage(Xamarin.Forms.Page page)
        {
            Page = page;
            Identifier = Guid.NewGuid().ToString();
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Page = null;
            }

            base.Dispose();
        }

        public string Identifier { get; set; }

        public Xamarin.Forms.Page Page { get; private set; }
    }
}