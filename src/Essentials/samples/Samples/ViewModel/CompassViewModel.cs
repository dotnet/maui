using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	class CompassViewModel : BaseViewModel
	{
		bool compass1IsActive;
		bool compass2IsActive;
		bool applyLowPassFilter;
		double compass1;
		double compass2;
		int speed1 = 0;
		int speed2 = 0;

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

		public bool ApplyLowPassFilter
		{
			get => applyLowPassFilter;
			set
			{
				SetProperty(ref applyLowPassFilter, value);
			}
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

		public string[] Speeds { get; } =
		   Enum.GetNames(typeof(SensorSpeed));

		public override void OnDisappearing()
		{
			OnStopCompass1();
			OnStopCompass2();

			base.OnDisappearing();
		}

		async void OnStartCompass1()
		{
			try
			{
				if (Compass.IsMonitoring)
					OnStopCompass2();

				Compass.Start((SensorSpeed)Speed1, ApplyLowPassFilter);
				Compass.ReadingChanged += OnCompass1ReadingChanged;
				Compass1IsActive = true;
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to start compass: {ex.Message}");
			}
		}

		void OnCompass1ReadingChanged(object sender, CompassChangedEventArgs e)
		{
			switch ((SensorSpeed)Speed1)
			{
				case SensorSpeed.Fastest:
				case SensorSpeed.Game:
					MainThread.BeginInvokeOnMainThread(() => { Compass1 = e.Reading.HeadingMagneticNorth; });
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
			Compass.ReadingChanged -= OnCompass1ReadingChanged;
		}

		async void OnStartCompass2()
		{
			try
			{
				if (Compass.IsMonitoring)
					OnStopCompass1();

				Compass.Start((SensorSpeed)Speed2, ApplyLowPassFilter);
				Compass.ReadingChanged += OnCompass2ReadingChanged;
				Compass2IsActive = true;
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to start compass: {ex.Message}");
			}
		}

		void OnCompass2ReadingChanged(object sender, CompassChangedEventArgs e)
		{
			var data = e.Reading;
			switch ((SensorSpeed)Speed2)
			{
				case SensorSpeed.Fastest:
				case SensorSpeed.Game:
					MainThread.BeginInvokeOnMainThread(() => { Compass2 = data.HeadingMagneticNorth; });
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
			Compass.ReadingChanged -= OnCompass2ReadingChanged;
		}
	}
}
