using Xamarin.Essentials;

namespace Samples.ViewModel
{
    public class BatteryViewModel : BaseViewModel
    {
        public BatteryViewModel()
        {
        }

        public double Level => Battery.ChargeLevel;

        public BatteryState State => Battery.State;

        public BatteryPowerSource PowerSource => Battery.PowerSource;

        public override void OnAppearing()
        {
            base.OnAppearing();

            Battery.BatteryChanged += OnBatteryChanged;
        }

        public override void OnDisappearing()
        {
            Battery.BatteryChanged -= OnBatteryChanged;

            base.OnDisappearing();
        }

        void OnBatteryChanged(BatteryChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(PowerSource));
        }
    }
}
