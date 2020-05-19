using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Maui.DualScreen
{
	public partial class DualScreenInfo : INotifyPropertyChanged
	{
		public Task<int> GetHingeAngleAsync() => DualScreenService.GetHingeAngleAsync();

		void ProcessHingeAngleSubscriberCount(int newCount) { }
	}
}
