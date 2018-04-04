using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class MagnetometerViewModel : BaseViewModel
    {
        public MagnetometerViewModel()
        {
            StartCommand = new Command(OnStart);
            StopCommand = new Command(OnStop);
            Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
        }

        private void Magnetometer_ReadingChanged(MagnetometerChangedEventArgs e)
        {
            var data = e.Reading;
            switch ((SensorSpeed)Speed)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    Platform.BeginInvokeOnMainThread(() =>
                    {
                        X = data.MagneticFieldX;
                        Y = data.MagneticFieldY;
                        Z = data.MagneticFieldZ;
                    });
                    break;
                default:
                    X = data.MagneticFieldX;
                    Y = data.MagneticFieldY;
                    Z = data.MagneticFieldZ;
                    break;
            }
        }

        async void OnStart()
        {
            try
            {
                Magnetometer.Start((SensorSpeed)Speed);
                IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Magnetometer not supported");
            }
        }

        void OnStop()
        {
            IsActive = false;
            Magnetometer.Stop();
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        double x;

        public double X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        double y;

        public double Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        double z;

        public double Z
        {
            get => z;
            set => SetProperty(ref z, value);
        }

        bool isActive;

        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        public List<string> Speeds { get; } =
           new List<string>
           {
                "Fastest",
                "Game",
                "Normal",
                "User Interface"
           };

        int speed = 2;

        public int Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        public override void OnDisappearing()
        {
            OnStop();
            Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
            base.OnDisappearing();
        }
    }
}
