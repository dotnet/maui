using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace Samples.ViewModel
{
	public class BarometerViewModel : BaseViewModel
	{
		bool isActive;
		double pressure;
		int speed = 0;

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

		public string[] Speeds { get; } =
			Enum.GetNames(typeof(SensorSpeed));

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

		void OnStop()
		{
			try
			{
				Barometer.Stop();
				IsActive = false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to stop barometer: {0}", ex);
			}
		}

		void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
		{
			Pressure = e.Reading.PressureInHectopascals;
		}
	}
}
