using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using ContactsManager = Microsoft.Maui.ApplicationModel.Communication.Contacts;

namespace Samples.ViewModel
{
	class ContactsViewModel : BaseViewModel
	{
		Contact selectedContact;

		public ContactsViewModel()
		{
			GetContactCommand = new Command(OnGetContact);
			GetAllContactCommand = new Command(OnGetAllContact);
		}

		public ICommand GetContactCommand { get; }

		public ICommand GetAllContactCommand { get; }

		public ObservableCollection<Contact> ContactsList { get; } = new ObservableCollection<Contact>();

		public Contact SelectedContact
		{
			get => selectedContact;
			set => SetProperty(ref selectedContact, value, onChanged: OnContactSelected);
		}

		async void OnGetContact()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
				var contact = await ContactsManager.PickContactAsync();
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

		async void OnGetAllContact()
		{
			if (await Permissions.RequestAsync<Permissions.ContactsRead>() != PermissionStatus.Granted)
				return;

			if (IsBusy)
				return;
			IsBusy = true;

			ContactsList.Clear();
			try
			{
				var contacts = await ContactsManager.GetAllAsync();

				await Task.Run(() =>
				{
					foreach (var contact in contacts)
					{
						MainThread.BeginInvokeOnMainThread(() => ContactsList.Add(contact));
					}
				});
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Error:{ex.Message}");
			}
			finally
			{
				IsBusy = false;
			}
		}

		async void OnContactSelected()
		{
			if (SelectedContact == null)
				return;

			var details = new ContactDetailsViewModel(SelectedContact);

			SelectedContact = null;

			await NavigateAsync(details);
		}
	}
}
