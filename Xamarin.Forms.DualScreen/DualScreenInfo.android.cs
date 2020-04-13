using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Device.Display;

namespace Xamarin.Forms.DualScreen
{
	public partial class DualScreenInfo : INotifyPropertyChanged
	{
		static object hingeAngleLock = new object();
		public Task<int> GetHingeAngleAsync() => DualScreenService.GetHingeAngleAsync();

		void ProcessHingeAngleSubscriberCount(int newCount)
		{
			lock (hingeAngleLock)
			{
				if (newCount == 1)
				{
					DualScreen.DualScreenService.DualScreenServiceImpl.HingeAngleChanged += OnHingeAngleChanged;
				}
				else if (newCount == 0)
				{
					DualScreen.DualScreenService.DualScreenServiceImpl.HingeAngleChanged -= OnHingeAngleChanged;
				}
			}
		}

		void OnHingeAngleChanged(object sender, HingeSensor.HingeSensorChangedEventArgs e)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				_hingeAngleChanged?.Invoke(this, new HingeAngleChangedEventArgs(e.HingeAngle));
			});
		}
	}
}
