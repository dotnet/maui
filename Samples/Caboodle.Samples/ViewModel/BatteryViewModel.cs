using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class BatteryViewModel : BaseViewModel
    {
        public string Level => $"Charge Level: {Battery.ChargeLevel}";

        public string State => $"State: {Battery.State}";

        public string PowerSource => $"Power Source: {Battery.PowerSource}";

        public string ListenText { get; set; } = "Listen for events";

        public ICommand RefreshStatsCommand { get; }

        public ICommand ListenCommand { get; }

        bool listening;

        public BatteryViewModel()
        {
            RefreshStatsCommand = new Command(() =>
            {
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(PowerSource));
            });

            ListenCommand = new Command(() =>
            {
                if (!listening)
                {
                    Battery.BatteryChanged += Battery_BatteryChanged;
                    ListenText = "Stop listening";
                }
                else
                {
                    Battery.BatteryChanged -= Battery_BatteryChanged;
                    ListenText = "Listen for events";
                }

                listening = !listening;
                OnPropertyChanged(nameof(ListenText));
            });
        }

        async void Battery_BatteryChanged(BatteryChangedEventArgs e)
        {
            RefreshStatsCommand.Execute(null);
            await Application.Current.MainPage.DisplayAlert("Battery Event", $"Battery status changes\n Level: {e.ChargeLevel}\n Source: {e.PowerSource}\n State: {e.State}", "OK");
        }

        public void Stop()
        {
            if (listening)
            {
                Battery.BatteryChanged -= Battery_BatteryChanged;
            }
        }
    }
}
