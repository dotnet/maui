using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class MagnetometerViewModel : BaseViewModel
    {
        double x;
        double y;
        double z;
        bool isActive;
        int speed = 2;

        public MagnetometerViewModel()
        {
            StartCommand = new Command(OnStart);
            StopCommand = new Command(OnStop);

            Magnetometer.ReadingChanged += OnReadingChanged;
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public double X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        public double Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        public double Z
        {
            get => z;
            set => SetProperty(ref z, value);
        }

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

        public int Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        public override void OnDisappearing()
        {
            OnStop();
            Magnetometer.ReadingChanged -= OnReadingChanged;

            base.OnDisappearing();
        }

        void OnReadingChanged(MagnetometerChangedEventArgs e)
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
    }
}
