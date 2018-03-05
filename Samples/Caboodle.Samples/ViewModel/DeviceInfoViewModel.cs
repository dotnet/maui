using Microsoft.Caboodle;
using MvvmHelpers;

namespace Caboodle.Samples.ViewModel
{
    public class DeviceInfoViewModel : BaseViewModel
    {
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

        public ScreenMetrics ScreenMetrics { get; set; } = DeviceInfo.ScreenMetrics;

        public void UpdateScreenMetrics(ScreenMetrics newMetrics)
        {
            ScreenMetrics = newMetrics;
            OnPropertyChanged(nameof(ScreenMetrics));
        }
    }
}
