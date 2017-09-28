using System;

namespace Xamarin.Forms.Platform.GTK.Extensions
{
    public static class PageExtensions
    {
        internal static bool ShouldDisplayNativeWindow(this Page page)
        {
            var parentPage = page.Parent as Page;

            if (parentPage != null)
            {
                return string.IsNullOrEmpty(parentPage.BackgroundImage);
            }

            return true;
        }

        public static Gtk.EventBox CreateContainer(this Page view)
        {
            if (!Forms.IsInitialized)
                throw new InvalidOperationException("call Forms.Init() before this");

            if (!(view.RealParent is Application))
            {
                Application app = new DefaultApplication();
                app.MainPage = view;
            }

            var result = new Platform();
            result.SetPage(view);

            return result.PlatformRenderer;
        }

        class DefaultApplication : Application
        {
        }
    }
}