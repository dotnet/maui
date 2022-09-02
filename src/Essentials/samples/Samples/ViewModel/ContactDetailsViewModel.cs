using Microsoft.Maui.ApplicationModel.Communication;

namespace Samples.ViewModel
{
	class ContactDetailsViewModel : BaseViewModel
	{
		public ContactDetailsViewModel(Contact contact)
		{
			Contact = contact;
		}

		public Contact Contact { get; }
	}
}
