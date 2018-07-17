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

        public EnergySaverStatus EnergySaverStatus => Power.EnergySaverStatus;

        public override void OnAppearing()
        {
            base.OnAppearing();

            Battery.BatteryChanged += OnBatteryChanged;
            Power.EnergySaverStatusChanaged += OnEnergySaverStatusChanaged;
        }

        public override void OnDisappearing()
        {
            Battery.BatteryChanged -= OnBatteryChanged;
            Power.EnergySaverStatusChanaged -= OnEnergySaverStatusChanaged;

            base.OnDisappearing();
        }

        void OnEnergySaverStatusChanaged(EnergySaverStatusChanagedEventArgs e)
        {
            OnPropertyChanged(nameof(EnergySaverStatus));
        }

        void OnBatteryChanged(BatteryChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(PowerSource));
        }
    }
}
