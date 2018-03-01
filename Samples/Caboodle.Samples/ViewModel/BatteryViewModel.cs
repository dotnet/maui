using Microsoft.Caboodle;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class BatteryViewModel : BaseViewModel
    {
        public string Level => $"Charge Level: {Battery.ChargeLevel}";
        public string State => $"State: {Battery.State}";
        public string PowerSource => $"Power Source: {Battery.PowerSource}";

        public ICommand RefreshStatsCommand { get; }
        public BatteryViewModel()
        {
            RefreshStatsCommand = new Command(() =>
            {
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(PowerSource));
            });
        }
    }
}
