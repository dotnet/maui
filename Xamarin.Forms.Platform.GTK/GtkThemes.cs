using Gtk;
using System;
using System.Diagnostics;
using System.IO;
using Xamarin.Forms.Platform.GTK.Helpers;

namespace Xamarin.Forms.Platform.GTK
{
    public static class GtkThemes
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool SetDllDirectory(string lpPathName);

        public static bool IsInitialized { get; private set; }

        public static void Init()
        {
            if (IsInitialized)
                return;

            if (PlatformHelper.GetGTKPlatform() == GTKPlatform.Windows)
                CheckWindowsGtk();

            IsInitialized = true;
        }

        public static void LoadCustomTheme(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            if (!IsInitialized)
                throw new InvalidOperationException("call GtkThemes.Init() before this");

            // GTK provides resource file mechanism for configuring various aspects of the operation of a GTK program at runtime. 
            // Parses resource information from a string to allow change the App appearance.
            Rc.Parse(filename);
        }

        private static bool CheckWindowsGtk()
        {
            string location = null;
            Version version = null;
            Version minVersion = new Version(2, 12, 22);

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\InstallFolder"))
            {
                if (key != null)
                    location = key.GetValue(null) as string;
            }
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\Version"))
            {
                if (key != null)
                    Version.TryParse(key.GetValue(null) as string, out version);
            }

            if (version == null || version < minVersion || location == null || !File.Exists(System.IO.Path.Combine(location, "bin", "libgtk-win32-2.0-0.dll")))
            {
                return false;
            }

            var path = Path.Combine(location, @"bin");
            try
            {
                if (SetDllDirectory(path))
                {
                    return true;
                }
            }
            catch (EntryPointNotFoundException)
            {
            }

            return true;
        }
    }
}