using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class SmsViewModel : BaseViewModel
    {
        string recipient;
        string messageText;

        public SmsViewModel()
        {
            SendSmsCommand = new Command(OnSendSms);
        }

        public string Recipient
        {
            get => recipient;
            set => base.SetProperty(ref recipient, value);
        }

        public string MessageText
        {
            get => messageText;
            set => SetProperty(ref messageText, value);
        }

        public ICommand SendSmsCommand { get; }

        async void OnSendSms()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var message = new SmsMessage(MessageText, Recipient);
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Sending an SMS is not supported on this device.");
            }

            IsBusy = false;
        }
    }
}
