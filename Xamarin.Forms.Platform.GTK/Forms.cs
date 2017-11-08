using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK;

namespace Xamarin.Forms
{
    public static class Forms
    {
        internal static string BarTextColor = "Xamarin.BarTextColor";
        internal static string BarBackgroundColor = "Xamarin.BarBackgroundColor";

        const string LogFormat = "[{0}] {1}";

        public static bool IsInitialized { get; private set; }

        public static void Init(IEnumerable<Assembly> rendererAssemblies = null)
        {
            if (IsInitialized)
                return;

            Log.Listeners.Add(new DelegateLogListener((c, m) => Debug.WriteLine(LogFormat, c, m)));

            Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

            IsInitialized = true;

            Device.SetIdiom(TargetIdiom.Desktop);
            Device.PlatformServices = new GtkPlatformServices();
            Device.Info = new GtkDeviceInfo();
            Color.SetAccent(Color.FromHex("#3498DB"));
            ExpressionSearch.Default = new GtkExpressionSearch();

            Registrar.RegisterAll(new[]
            {
                typeof(ExportCellAttribute),
                typeof(ExportImageSourceHandlerAttribute),
                typeof(ExportRendererAttribute)
            });
        }
    }
}
