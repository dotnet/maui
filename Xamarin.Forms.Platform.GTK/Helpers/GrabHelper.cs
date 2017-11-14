using Gtk;

namespace Xamarin.Forms.Platform.GTK.Helpers
{
    public class GrabHelper
    {
        private static uint CURRENT_TIME = 0;

        public static void GrabWindow(Window window)
        {
            window.GrabFocus();

            Grab.Add(window);

            Gdk.GrabStatus grabbed =
                Gdk.Pointer.Grab(window.GdkWindow, true,
                Gdk.EventMask.ButtonPressMask
                | Gdk.EventMask.ButtonReleaseMask
                | Gdk.EventMask.PointerMotionMask, null, null, CURRENT_TIME);

            if (grabbed == Gdk.GrabStatus.Success)
            {
                grabbed = Gdk.Keyboard.Grab(window.GdkWindow, true, CURRENT_TIME);

                if (grabbed != Gdk.GrabStatus.Success)
                {
                    Grab.Remove(window);
                    window.Destroy();
                }
            }
            else
            {
                Grab.Remove(window);
                window.Destroy();
            }
        }

        public static void RemoveGrab(Window window)
        {
            Grab.Remove(window);
            Gdk.Pointer.Ungrab(CURRENT_TIME);
            Gdk.Keyboard.Ungrab(CURRENT_TIME);
        }
    }
}