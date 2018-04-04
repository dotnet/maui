using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class GyroscopeViewModel : BaseViewModel
    {
        public GyroscopeViewModel()
        {
            StartCommand = new Command(OnStart);
            StopCommand = new Command(OnStop);
            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
        }

        private void Gyroscope_ReadingChanged(GyroscopeChangedEventArgs e)
        {
            var data = e.Reading;
            switch ((SensorSpeed)Speed)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    Platform.BeginInvokeOnMainThread(() =>
                    {
                        X = data.AngularVelocityX;
                        Y = data.AngularVelocityY;
                        Z = data.AngularVelocityZ;
                    });
                    break;
                default:
                    X = data.AngularVelocityX;
                    Y = data.AngularVelocityY;
                    Z = data.AngularVelocityZ;
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
            Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
            base.OnDisappearing();
        }
    }
}
