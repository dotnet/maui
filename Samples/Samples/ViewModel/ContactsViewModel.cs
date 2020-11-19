using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class ContactsViewModel : BaseViewModel
    {
        public ContactsViewModel()
        {
            GetContactCommand = new Command(OnGetContact);
        }

        public ICommand GetContactCommand { get; }

        async void OnGetContact()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                var contact = await Contacts.PickContactAsync();
                if (contact == null)
                    return;

                var details = new ContactDetailsViewModel(contact);
                await NavigateAsync(details);
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () => await DisplayAlertAsync($"Error:{ex.Message}"));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
