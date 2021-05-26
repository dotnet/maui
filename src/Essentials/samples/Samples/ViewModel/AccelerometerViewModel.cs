using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Samples.ViewModel
{
	public class AccelerometerViewModel : BaseViewModel
	{
		double x;
		double y;
		double z;
		string shakeTime = string.Empty;
		bool isActive;
		int speed = 0;

		public AccelerometerViewModel()
		{
			StartCommand = new Command(OnStart);
			StopCommand = new Command(OnStop);
		}

		public ICommand StartCommand { get; }

		public ICommand StopCommand { get; }

		public string ShakeTime
		{
			get => shakeTime;
			set => SetProperty(ref shakeTime, value);
		}

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

		public string[] Speeds { get; } =
		   Enum.GetNames(typeof(SensorSpeed));

		public int Speed
		{
			get => speed;
			set => SetProperty(ref speed, value);
		}

		public override void OnAppearing()
		{
			Accelerometer.ReadingChanged += OnReadingChanged;
			Accelerometer.ShakeDetected += Accelerometer_OnShaked;

			base.OnAppearing();
		}

		void Accelerometer_OnShaked(object sender, EventArgs e) =>
			ShakeTime = $"Shake detected: {DateTime.Now.ToLongTimeString()}";

		public override void OnDisappearing()
		{
			OnStop();
			Accelerometer.ReadingChanged -= OnReadingChanged;
			Accelerometer.ShakeDetected -= Accelerometer_OnShaked;
			base.OnDisappearing();
		}

		async void OnStart()
		{
			try
			{
				Accelerometer.Start((SensorSpeed)Speed);
				IsActive = true;
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to start accelerometer: {ex.Message}");
			}
		}

		void OnStop()
		{
			IsActive = false;
			Accelerometer.Stop();
		}

		void OnReadingChanged(object sender, AccelerometerChangedEventArgs e)
		{
			var data = e.Reading;
			switch ((SensorSpeed)Speed)
			{
				case SensorSpeed.Fastest:
				case SensorSpeed.Game:
					MainThread.BeginInvokeOnMainThread(() =>
					{
						X = data.Acceleration.X;
						Y = data.Acceleration.Y;
						Z = data.Acceleration.Z;
					});
					break;
				default:
					X = data.Acceleration.X;
					Y = data.Acceleration.Y;
					Z = data.Acceleration.Z;
					break;
			}
		}
	}
}
