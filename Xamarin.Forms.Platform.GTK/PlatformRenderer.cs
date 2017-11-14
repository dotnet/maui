using Gtk;

namespace Xamarin.Forms.Platform.GTK
{
    internal class PlatformRenderer : EventBox
    {
        public PlatformRenderer(Platform platform)
        {
            Platform = platform;
        }

        public Platform Platform { get; set; }
    }
}