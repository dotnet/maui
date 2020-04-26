using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class HaptickFeadbackViewModel : BaseViewModel
    {
        bool isSupported = true;

        public HaptickFeadbackViewModel()
        {
            ClickCommand = new Command(OnClick);
            LongPressCommand = new Command(OnLongPress);
        }

        public ICommand ClickCommand { get; }

        public ICommand LongPressCommand { get; }

        public bool IsSupported
        {
            get => isSupported;
            set => SetProperty(ref isSupported, value);
        }

        void OnClick()
        {
            try
            {
                HapticFeedback.Execute(HapticFeedbackType.Click);
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }
            catch (Exception ex)
            {
                DisplayAlertAsync($"Unable to HapticFeedback: {ex.Message}");
            }
        }

        void OnLongPress()
        {
            try
            {
                HapticFeedback.Execute(HapticFeedbackType.LongPress);
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }
            catch (Exception ex)
            {
                DisplayAlertAsync($"Unable to HapticFeedback: {ex.Message}");
            }
        }
    }
}
