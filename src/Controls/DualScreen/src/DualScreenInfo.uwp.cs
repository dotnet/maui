using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace Microsoft.Maui.Controls.DualScreen
{
	public partial class DualScreenInfo : INotifyPropertyChanged
	{
		static object hingeAngleLock = new object();
		public Task<int> GetHingeAngleAsync() => DualScreenService.GetHingeAngleAsync();

#if UWP_18362
		Windows.Devices.Sensors.HingeAngleSensor _angleSensor;
#endif

#if UWP_18362
		async void ProcessHingeAngleSubscriberCount(int newCount)
		{
			try
			{
				if (_angleSensor == null)
					_angleSensor = await Windows.Devices.Sensors.HingeAngleSensor.GetDefaultAsync();

				if (_angleSensor == null)
					return;

				lock (hingeAngleLock)
				{
					if (newCount == 1)
					{
						_angleSensor.ReadingChanged += OnReadingChanged;
					}
					else if(newCount == 0)
					{
						_angleSensor.ReadingChanged -= OnReadingChanged;
					}
				}
			}
			catch(Exception e)
			{
				Internals.Log.Warning(nameof(DualScreenInfo), $"Failed to retrieve Hinge Angle Sensor {e}");
			}

			void OnReadingChanged(HingeAngleSensor sender, HingeAngleSensorReadingChangedEventArgs args)
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					_hingeAngleChanged?.Invoke(this, new HingeAngleChangedEventArgs(args.Reading.AngleInDegrees));
				});
			}
		}
#else
		void ProcessHingeAngleSubscriberCount(int newCount)
		{
		}
#endif
	}
}
