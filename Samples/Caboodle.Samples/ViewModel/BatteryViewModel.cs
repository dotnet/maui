using Microsoft.Caboodle;

namespace Caboodle.Samples.ViewModel
{
    public class BatteryViewModel : BaseViewModel
    {
        public BatteryViewModel()
        {
        }

        public double Level => Battery.ChargeLevel;

        public BatteryState State => Battery.State;

        public BatteryPowerSource PowerSource => Battery.PowerSource;

        public void Update(BatteryChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(PowerSource));
        }
    }
}
