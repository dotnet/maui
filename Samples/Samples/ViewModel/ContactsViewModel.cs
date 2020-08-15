using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class ContactsViewModel : BaseViewModel
    {
        string name;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        string birthday;

        public string Birthday
        {
            get => birthday;
            set => SetProperty(ref birthday, value);
        }

        string phones;

        public string Phones
        {
            get => phones;
            set => SetProperty(ref phones, value);
        }

        string emails;

        public string Emails
        {
            get => emails;
            set => SetProperty(ref emails, value);
        }

        string contactType;

        public string ContactType
        {
            get => contactType;
            set => SetProperty(ref contactType, value);
        }

        public ICommand GetContactCommand { get; }

        public ContactsViewModel()
        {
            GetContactCommand = new Command(OnGetContact);
        }

        async void OnGetContact()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                Phones = string.Empty;
                Emails = string.Empty;
                Name = string.Empty;
                Birthday = string.Empty;
                ContactType = string.Empty;

                var contact = await Contacts.PickContactAsync();
                if (contact == null)
                    return;

                foreach (var number in contact?.Numbers)
                    Phones += number.PhoneNumber + number.ContactType + Environment.NewLine;

                foreach (var email in contact?.Emails)
                    Emails += email.EmailAddress + email.ContactType + Environment.NewLine;

                Name = contact?.Name;
                Birthday = contact?.Birthday.ToString();
                ContactType = contact?.ContactType.ToString();
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
