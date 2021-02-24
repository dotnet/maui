using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	public class KeepScreenOnViewModel : BaseViewModel
	{
		public KeepScreenOnViewModel()
		{
			RequestActiveCommand = new Command(OnRequestActive);
			RequestReleaseCommand = new Command(OnRequestRelease);
		}

		public bool IsActive => DeviceDisplay.KeepScreenOn;

		public ICommand RequestActiveCommand { get; }

		public ICommand RequestReleaseCommand { get; }

		void OnRequestActive()
		{
			DeviceDisplay.KeepScreenOn = true;

			OnPropertyChanged(nameof(IsActive));
		}

		void OnRequestRelease()
		{
			DeviceDisplay.KeepScreenOn = false;

			OnPropertyChanged(nameof(IsActive));
		}
	}
}
