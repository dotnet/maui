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

		public EnergySaverStatus EnergySaverStatus => Battery.EnergySaverStatus;

		public override void OnAppearing()
		{
			base.OnAppearing();

			Battery.BatteryInfoChanged += OnBatteryInfoChanged;
			Battery.EnergySaverStatusChanged += OnEnergySaverStatusChanged;
		}

		public override void OnDisappearing()
		{
			Battery.BatteryInfoChanged -= OnBatteryInfoChanged;
			Battery.EnergySaverStatusChanged -= OnEnergySaverStatusChanged;

			base.OnDisappearing();
		}

		void OnEnergySaverStatusChanged(object sender, EnergySaverStatusChangedEventArgs e)
		{
			OnPropertyChanged(nameof(EnergySaverStatus));
		}

		void OnBatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Level));
			OnPropertyChanged(nameof(State));
			OnPropertyChanged(nameof(PowerSource));
		}
	}
}
