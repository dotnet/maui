using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace Samples.ViewModel
{
	class CompassViewModel : BaseViewModel
	{
		bool isActive;
		bool applyLowPassFilter;
		double heading;
		int speed = 0;

		public CompassViewModel()
		{
			StartCommand = new Command(OnStart);
			StopCommand = new Command(OnStop);
		}

		public ICommand StartCommand { get; }

		public ICommand StopCommand { get; }

		public bool IsActive
		{
			get => isActive;
			set => SetProperty(ref isActive, value);
		}

		public bool ApplyLowPassFilter
		{
			get => applyLowPassFilter;
			set => SetProperty(ref applyLowPassFilter, value);
		}

		public double Heading
		{
			get => heading;
			set => SetProperty(ref heading, value);
		}

		public int Speed
		{
			get => speed;
			set => SetProperty(ref speed, value);
		}

		public string[] Speeds { get; } =
			Enum.GetNames(typeof(SensorSpeed));

		public override void OnAppearing()
		{
			Compass.ReadingChanged += OnCompassReadingChanged;

			base.OnAppearing();
		}

		public override void OnDisappearing()
		{
			OnStop();

			Compass.ReadingChanged -= OnCompassReadingChanged;

			base.OnDisappearing();
		}

		async void OnStart()
		{
			try
			{
				Compass.Start((SensorSpeed)Speed, ApplyLowPassFilter);
				IsActive = true;
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to start compass: {ex.Message}");
			}
		}

		void OnStop()
		{
			try
			{
				Compass.Stop();
				IsActive = false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to stop compass: {0}", ex);
			}
		}

		void OnCompassReadingChanged(object sender, CompassChangedEventArgs e)
		{
			switch ((SensorSpeed)Speed)
			{
				case SensorSpeed.Fastest:
				case SensorSpeed.Game:
					MainThread.BeginInvokeOnMainThread(() => Heading = e.Reading.HeadingMagneticNorth);
					break;
				default:
					Heading = e.Reading.HeadingMagneticNorth;
					break;
			}
		}
	}
}
