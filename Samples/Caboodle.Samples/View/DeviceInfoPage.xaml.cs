using Caboodle.Samples.ViewModel;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.View
{
    public partial class DeviceInfoPage : ContentPage
    {
        public DeviceInfoPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            DeviceInfo.ScreenMetricsChanaged += OnScreenMetricsChanged;
        }

        protected override void OnDisappearing()
        {
            DeviceInfo.ScreenMetricsChanaged -= OnScreenMetricsChanged;

            base.OnDisappearing();
        }

        void OnScreenMetricsChanged(ScreenMetricsChanagedEventArgs e)
        {
            if (BindingContext is DeviceInfoViewModel vm)
            {
                vm.ScreenMetrics = e.Metrics;
            }
        }
    }
}
