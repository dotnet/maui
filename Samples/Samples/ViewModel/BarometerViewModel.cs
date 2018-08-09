using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class BarometerViewModel : BaseViewModel
    {
        bool barometerIsActive;
        double barometer;

        public BarometerViewModel()
        {
            StartBarometerComand = new Command(OnStartBarometer);
            StopBarometerCommand = new Command(OnStopBarometer);
        }

        public ICommand StartBarometerComand { get; }

        public ICommand StopBarometerCommand { get; }

        public bool BarometerIsActive
        {
            get => barometerIsActive;
            set => SetProperty(ref barometerIsActive, value);
        }

        public double BaromterValue
        {
            get => barometer;
            set => SetProperty(ref barometer, value);
        }

        public override void OnDisappearing()
        {
            OnStopBarometer();

            base.OnDisappearing();
        }

        async void OnStartBarometer()
        {
            try
            {
                Barometer.ReadingChanged += OnBarometerReadingChanged;

                Barometer.Start();
                BarometerIsActive = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Unable to start barometer: {ex.Message}");
            }
        }

        void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            BaromterValue = e.BarometerData.Pressure;
        }

        void OnStopBarometer()
        {
            BarometerIsActive = false;
            Barometer.Stop();
            Barometer.ReadingChanged -= OnBarometerReadingChanged;
        }
    }
}
