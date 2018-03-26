using System;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class PhoneDialerViewModel : BaseViewModel
    {
        string phoneNumber;

        public PhoneDialerViewModel()
        {
            OpenPhoneDialerCommand = new Command(OnOpenPhoneDialer);
        }

        public ICommand OpenPhoneDialerCommand { get; }

        public string PhoneNumber
        {
            get => phoneNumber;
            set => SetProperty(ref phoneNumber, value);
        }

        async void OnOpenPhoneDialer()
        {
            try
            {
                PhoneDialer.Open(PhoneNumber);
            }
            catch (Exception)
            {
                await DisplayAlert("Dialer is not supported.");
            }
        }
    }
}
