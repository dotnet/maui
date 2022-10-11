using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace Samples.ViewModel
{
	public class GyroscopeViewModel : BaseViewModel
	{
		double x;
		double y;
		double z;
		bool isActive;
		int speed = 0;

		public GyroscopeViewModel()
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
			Gyroscope.ReadingChanged += OnReadingChanged;

			base.OnAppearing();
		}

		public override void OnDisappearing()
		{
			OnStop();

			Gyroscope.ReadingChanged -= OnReadingChanged;

			base.OnDisappearing();
		}

		async void OnStart()
		{
			try
			{
				Gyroscope.Start((SensorSpeed)Speed);
				IsActive = true;
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to start gyroscope: {ex.Message}");
			}
		}

		void OnStop()
		{
			try
			{
				Gyroscope.Stop();
				IsActive = false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to stop gyroscope: {0}", ex);
			}
		}

		void OnReadingChanged(object sender, GyroscopeChangedEventArgs e)
		{
			var data = e.Reading;
			switch ((SensorSpeed)Speed)
			{
				case SensorSpeed.Fastest:
				case SensorSpeed.Game:
					MainThread.BeginInvokeOnMainThread(() =>
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
	}
}
