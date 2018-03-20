using Microsoft.Caboodle;

namespace Caboodle.Samples.ViewModel
{
    public class DeviceInfoViewModel : BaseViewModel
    {
        ScreenMetrics screenMetrics;

        public DeviceInfoViewModel()
        {
        }

        public string Model => DeviceInfo.Model;

        public string Manufacturer => DeviceInfo.Manufacturer;

        public string Name => DeviceInfo.Name;

        public string Version => DeviceInfo.VersionString;

        public string AppPackageName => AppInfo.PackageName;

        public string AppName => AppInfo.Name;

        public string AppVersion => AppInfo.VersionString;

        public string AppBuild => AppInfo.BuildString;

        public string Platform => DeviceInfo.Platform;

        public string Idiom => DeviceInfo.Idiom;

        public DeviceType DeviceType => DeviceInfo.DeviceType;

        public ScreenMetrics ScreenMetrics
        {
            get => screenMetrics;
            set => SetProperty(ref screenMetrics, value);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            DeviceInfo.ScreenMetricsChanaged += OnScreenMetricsChanged;
            ScreenMetrics = DeviceInfo.ScreenMetrics;
        }

        public override void OnDisappearing()
        {
            DeviceInfo.ScreenMetricsChanaged -= OnScreenMetricsChanged;

            base.OnDisappearing();
        }

        void OnScreenMetricsChanged(ScreenMetricsChanagedEventArgs e)
        {
            ScreenMetrics = e.Metrics;
        }
    }
}
