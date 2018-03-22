using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class ScreenLockViewModel : BaseViewModel
    {
        public ScreenLockViewModel()
        {
            RequestActiveCommand = new Command(OnRequestActive);
            RequestReleaseCommand = new Command(OnRequestRelease);
        }

        public ICommand RequestActiveCommand { get; }

        public ICommand RequestReleaseCommand { get; }

        void OnRequestActive()
        {
            ScreenLock.RequestActive();

            OnPropertyChanged(nameof(IsActive));
        }

        void OnRequestRelease()
        {
            ScreenLock.RequestRelease();

            OnPropertyChanged(nameof(IsActive));
        }

        public bool IsActive => ScreenLock.IsActive;
    }
}
