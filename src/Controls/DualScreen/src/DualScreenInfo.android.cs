using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//HACK:FOLDABLE using Microsoft.Device.Display;

namespace Microsoft.Maui.Controls.DualScreen
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
					//HACK:FOLDABLE DualScreen.DualScreenService.DualScreenServiceImpl.HingeAngleChanged += OnHingeAngleChanged;
				}
				else if (newCount == 0)
				{
					//HACK:FOLDABLE DualScreen.DualScreenService.DualScreenServiceImpl.HingeAngleChanged -= OnHingeAngleChanged;
				}
			}
		}

		void OnHingeAngleChanged(object sender, HingeSensor.HingeSensorChangedEventArgs e)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				//HACK:FOLDABLE _hingeAngleChanged?.Invoke(this, new HingeAngleChangedEventArgs(e.HingeAngle));
			});
		}
	}
}
