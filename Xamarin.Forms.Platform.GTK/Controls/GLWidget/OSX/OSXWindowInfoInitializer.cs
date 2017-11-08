using System;
using System.Runtime.InteropServices;
using System.Security;
using OpenTK.Platform;

namespace OpenTK.OSX
{
    /// <summary>
    /// Handler class for initializing <see cref="IWindowInfo"/> objects under the OSX platform for both GTK2 and
    /// GTK3.
    /// </summary>
    public static class OSXWindowInfoInitializer
    {
        const string OSXLibGdkName = "libgdk-quartz-2.0.0.dylib";

        /// <summary>
        /// Initializes an <see cref="IWindowInfo"/> under the OSX platform.
        /// </summary>
        /// <param name="gdkWindowHandle"></param>
        public static IWindowInfo Initialize(IntPtr gdkWindowHandle)
        {
            IntPtr windowHandle = gdk_quartz_window_get_nswindow(gdkWindowHandle);
            IntPtr viewHandle = gdk_quartz_window_get_nsview(gdkWindowHandle);

            return Utilities.CreateMacOSWindowInfo(windowHandle, viewHandle);
        }

        [SuppressUnmanagedCodeSecurity, DllImport(OSXLibGdkName)]
        private static extern IntPtr gdk_quartz_window_get_nswindow(IntPtr handle);

        [SuppressUnmanagedCodeSecurity, DllImport(OSXLibGdkName)]
        private static extern IntPtr gdk_quartz_window_get_nsview(IntPtr handle);
    }
}