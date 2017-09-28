using System;
using System.Runtime.InteropServices;
using System.Security;
using OpenTK.Platform;

namespace OpenTK.Win
{
    /// <summary>
    /// Handler class for initializing <see cref="IWindowInfo"/> objects under the Windows platform for both GTK2 and
    /// GTK3.
    /// </summary>
    public static class WinWindowsInfoInitializer
    {
        private const string WinLibGDKName = "libgdk-win32-2.0-0.dll";

        /// <summary>
        /// Initializes an <see cref="IWindowInfo"/> under the Windows platform.
        /// </summary>
        /// <param name="gdkWindowHandle"></param>
        public static IWindowInfo Initialize(IntPtr gdkWindowHandle)
        {
            IntPtr windowHandle = gdk_win32_drawable_get_handle(gdkWindowHandle);

            return Utilities.CreateWindowsWindowInfo(windowHandle);
        }

        [SuppressUnmanagedCodeSecurity, DllImport(WinLibGDKName, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr gdk_win32_drawable_get_handle(IntPtr d);
    }
}