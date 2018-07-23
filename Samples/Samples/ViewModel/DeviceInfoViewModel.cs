using Xamarin.Essentials;

namespace Samples.ViewModel
{
    public class DeviceInfoViewModel : BaseViewModel
    {
        ScreenMetrics screenMetrics;

        public string Model => DeviceInfo.Model;

        public string Manufacturer => DeviceInfo.Manufacturer;

        public string Name => DeviceInfo.Name;

        public string Version => DeviceInfo.VersionString;

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

            DeviceDisplay.ScreenMetricsChanged += OnScreenMetricsChanged;
            ScreenMetrics = DeviceDisplay.ScreenMetrics;
        }

        public override void OnDisappearing()
        {
            DeviceDisplay.ScreenMetricsChanged -= OnScreenMetricsChanged;

            base.OnDisappearing();
        }

        void OnScreenMetricsChanged(object sender, ScreenMetricsChangedEventArgs e)
        {
            ScreenMetrics = e.Metrics;
        }
    }
}
