using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class FlashlightViewModel : BaseViewModel
    {
        private bool isOn;
        private bool isSupported = true;

        public FlashlightViewModel()
        {
            ToggleCommand = new Command(OnToggle);
        }

        public ICommand ToggleCommand { get; }

        public bool IsOn
        {
            get => isOn;
            set => SetProperty(ref isOn, value);
        }

        public bool IsSupported
        {
            get => isSupported;
            set => SetProperty(ref isSupported, value);
        }

        async void OnToggle()
        {
            try
            {
                if (IsOn)
                {
                    await Flashlight.TurnOffAsync();
                    IsOn = false;
                }
                else
                {
                    await Flashlight.TurnOnAsync();
                    IsOn = true;
                }
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }
        }

        public override void OnDisappearing()
        {
            try
            {
                Flashlight.TurnOffAsync();
                IsOn = false;
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }

            base.OnDisappearing();
        }
    }
}
