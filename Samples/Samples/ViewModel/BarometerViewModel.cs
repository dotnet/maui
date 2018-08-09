using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class BarometerViewModel : BaseViewModel
    {
        bool isActive;
        double pressure;
        int speed = 2;

        public BarometerViewModel()
        {
            StartCommand = new Command(OnStartBarometer);
            StopCommand = new Command(OnStop);
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        public double Pressure
        {
            get => pressure;
            set => SetProperty(ref pressure, value);
        }

        public List<string> Speeds { get; } =
           new List<string>
           {
                        "Fastest",
                        "Game",
                        "Normal",
                        "User Interface"
           };

        public int Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        public override void OnAppearing()
        {
            Barometer.ReadingChanged += OnBarometerReadingChanged;
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            OnStop();
            Barometer.ReadingChanged -= OnBarometerReadingChanged;

            base.OnDisappearing();
        }

        async void OnStartBarometer()
        {
            try
            {
                Barometer.Start((SensorSpeed)Speed);
                IsActive = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Unable to start barometer: {ex.Message}");
            }
        }

        void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            Pressure = e.Reading.Pressure;
        }

        void OnStop()
        {
            IsActive = false;
            Barometer.Stop();
            Barometer.ReadingChanged -= OnBarometerReadingChanged;
        }
    }
}
