using OpenTK;

namespace Xamarin.Forms.Platform.GTK
{
    public static class GtkOpenGL
    {
        public static bool IsInitialized { get; private set; }

        public static void Init()
        {
            if (IsInitialized)
                return;

            // Initializes OpenTK. This method is necessary because we are using OpenTK alongside a different windowing toolkit (GTK#).
            // Should be the very first method called by the application (i.e. calling this method should be the very first statement executed by the "Main" method).
            Toolkit.Init(new ToolkitOptions
            {
                Backend = PlatformBackend.PreferNative,
                EnableHighResolution = true
            });

            IsInitialized = true;
        }
    }
}