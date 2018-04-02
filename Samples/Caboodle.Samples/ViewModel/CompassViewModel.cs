using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    class CompassViewModel : BaseViewModel
    {
        bool compass1IsActive;
        bool compass2IsActive;
        double compass1;
        double compass2;
        int speed1 = 2;
        int speed2 = 2;

        public CompassViewModel()
        {
            StartCompass1Command = new Command(OnStartCompass1);
            StopCompass1Command = new Command(OnStopCompass1);
            StartCompass2Command = new Command(OnStartCompass2);
            StopCompass2Command = new Command(OnStopCompass2);
        }

        public ICommand StartCompass1Command { get; }

        public ICommand StopCompass1Command { get; }

        public ICommand StartCompass2Command { get; }

        public ICommand StopCompass2Command { get; }

        public bool Compass1IsActive
        {
            get => compass1IsActive;
            set => SetProperty(ref compass1IsActive, value);
        }

        public bool Compass2IsActive
        {
            get => compass2IsActive;
            set => SetProperty(ref compass2IsActive, value);
        }

        public double Compass1
        {
            get => compass1;
            set => SetProperty(ref compass1, value);
        }

        public double Compass2
        {
            get => compass2;
            set => SetProperty(ref compass2, value);
        }

        public int Speed1
        {
            get => speed1;
            set => SetProperty(ref speed1, value);
        }

        public int Speed2
        {
            get => speed2;
            set => SetProperty(ref speed2, value);
        }

        public List<string> CompassSpeeds { get; } =
            new List<string>
            {
                "Fastest",
                "Game",
                "Normal",
                "User Interface"
            };

        async void OnStartCompass1()
        {
            try
            {
                if (Compass.IsMonitoring)
                    OnStopCompass2();

                Compass.ReadingChanged += Compass1_ReadingChanged;

                Compass.Start((SensorSpeed)Speed1);
                Compass1IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Compass not supported");
            }
        }

        private void Compass1_ReadingChanged(CompassChangedEventArgs e)
        {
            switch ((SensorSpeed)Speed1)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    Platform.BeginInvokeOnMainThread(() => { Compass1 = e.Reading.HeadingMagneticNorth; });
                    break;
                default:
                    Compass1 = e.Reading.HeadingMagneticNorth;
                    break;
            }
        }

        void OnStopCompass1()
        {
            Compass1IsActive = false;
            Compass.Stop();
            Compass.ReadingChanged -= Compass1_ReadingChanged;
        }

        async void OnStartCompass2()
        {
            try
            {
                if (Compass.IsMonitoring)
                    OnStopCompass1();

                Compass.ReadingChanged += Compass2_ReadingChanged;
                Compass.Start((SensorSpeed)Speed2);
                Compass2IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Compass not supported");
            }
        }

        private void Compass2_ReadingChanged(CompassChangedEventArgs e)
        {
            var data = e.Reading;
            switch ((SensorSpeed)Speed2)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    Platform.BeginInvokeOnMainThread(() => { Compass2 = data.HeadingMagneticNorth; });
                    break;
                default:
                    Compass2 = data.HeadingMagneticNorth;
                    break;
            }
        }

        void OnStopCompass2()
        {
            Compass2IsActive = false;
            Compass.Stop();
            Compass.ReadingChanged -= Compass2_ReadingChanged;
        }

        public override void OnDisappearing()
        {
            OnStopCompass1();
            OnStopCompass2();

            base.OnDisappearing();
        }
    }
}
