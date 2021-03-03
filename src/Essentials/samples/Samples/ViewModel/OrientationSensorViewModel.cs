using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	class OrientationSensorViewModel : BaseViewModel
	{
		double x;
		double y;
		double z;
		double w;
		bool isActive;
		int speed = 0;

		public OrientationSensorViewModel()
		{
			StartCommand = new Command(OnStart);
			StopCommand = new Command(OnStop);
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

		public double W
		{
			get => w;
			set => SetProperty(ref w, value);
		}

		public bool IsActive
		{
			get => isActive;
			set => SetProperty(ref isActive, value);
		}

		public string[] Speeds { get; } =
		   Enum.GetNames(typeof(SensorSpeed));

		public int Speed
		{
			get => speed;
			set => SetProperty(ref speed, value);
		}

		public override void OnAppearing()
		{
			OrientationSensor.ReadingChanged += OnReadingChanged;
			base.OnAppearing();
		}

		public override void OnDisappearing()
		{
			OnStop();
			OrientationSensor.ReadingChanged -= OnReadingChanged;

			base.OnDisappearing();
		}

		async void OnStart()
		{
			try
			{
				OrientationSensor.Start((SensorSpeed)Speed);
				IsActive = true;
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to start orientation sensor: {ex.Message}");
			}
		}

		void OnStop()
		{
			IsActive = false;
			OrientationSensor.Stop();
		}

		void OnReadingChanged(object sender, OrientationSensorChangedEventArgs e)
		{
			var data = e.Reading;
			switch ((SensorSpeed)Speed)
			{
				case SensorSpeed.Fastest:
				case SensorSpeed.Game:
					MainThread.BeginInvokeOnMainThread(() =>
					{
						X = data.Orientation.X;
						Y = data.Orientation.Y;
						Z = data.Orientation.Z;
						W = data.Orientation.W;
					});
					break;
				default:
					X = data.Orientation.X;
					Y = data.Orientation.Y;
					Z = data.Orientation.Z;
					W = data.Orientation.W;
					break;
			}
		}
	}
}
