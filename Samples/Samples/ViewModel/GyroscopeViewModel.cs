using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class GyroscopeViewModel : BaseViewModel
    {
        double x;
        double y;
        double z;
        bool isActive;
        int speed = 2;

        public GyroscopeViewModel()
        {
            StartCommand = new Command(OnStart);
            StopCommand = new Command(OnStop);

            Gyroscope.ReadingChanged += OnReadingChanged;
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
            Gyroscope.ReadingChanged -= OnReadingChanged;

            base.OnDisappearing();
        }

        void OnReadingChanged(GyroscopeChangedEventArgs e)
        {
            var data = e.Reading;
            switch ((SensorSpeed)Speed)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    Platform.BeginInvokeOnMainThread(() =>
                    {
                        X = data.AngularVelocity.X;
                        Y = data.AngularVelocity.Y;
                        Z = data.AngularVelocity.Z;
                    });
                    break;
                default:
                    X = data.AngularVelocity.X;
                    Y = data.AngularVelocity.Y;
                    Z = data.AngularVelocity.Z;
                    break;
            }
        }

        async void OnStart()
        {
            try
            {
                Gyroscope.Start((SensorSpeed)Speed);
                IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Gyroscope not supported");
            }
        }

        void OnStop()
        {
            IsActive = false;
            Gyroscope.Stop();
        }
    }
}
