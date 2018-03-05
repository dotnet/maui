using Microsoft.Caboodle;

namespace Caboodle.Samples.ViewModel
{
    public class DeviceInfoViewModel : BaseViewModel
    {
        ScreenMetrics screenMetrics = DeviceInfo.ScreenMetrics;

        public DeviceInfoViewModel()
        {
        }

        public string Model => DeviceInfo.Model;

        public string Manufacturer => DeviceInfo.Manufacturer;

        public string Name => DeviceInfo.Name;

        public string Version => DeviceInfo.VersionString;

        public string AppPackageName => DeviceInfo.AppPackageName;

        public string AppName => DeviceInfo.AppName;

        public string AppVersion => DeviceInfo.AppVersionString;

        public string AppBuild => DeviceInfo.AppBuildString;

        public string Platform => DeviceInfo.Platform;

        public string Idiom => DeviceInfo.Idiom;

        public DeviceType DeviceType => DeviceInfo.DeviceType;

        public ScreenMetrics ScreenMetrics
        {
            get => screenMetrics;
            set => SetProperty(ref screenMetrics, value);
        }
    }
}
