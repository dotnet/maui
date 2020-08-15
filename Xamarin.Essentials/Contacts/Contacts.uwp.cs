using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        static async Task<Contact?> PlatformPickContactAsync()
        {
            var contactPicker = new ContactPicker();

            try
            {
                var contactSelected = await contactPicker.PickContactAsync();

                if (contactSelected == null)
                    throw new Exception("We can't get the contact!");

                var contactManager = await ContactManager.RequestStoreAsync();
                var contact = await contactManager.FindContactsAsync(contactSelected.Name);

                var phoneContact = contact[0];
                var phones = new List<ContactPhone>();
                var emails = new List<ContactEmail>();

                foreach (var item in phoneContact.Phones)
                    phones.Add(new ContactPhone(item.Number, GetPhoneContactType(item.Kind)));

                phones = phones.Distinct().ToList();

                foreach (var item in phoneContact.Emails)
                    emails.Add(new ContactEmail(item.Address, GetEmailContactType(item.Kind)));

                emails = emails.Distinct().ToList();

                var b = phoneContact.ImportantDates.FirstOrDefault(x => x.Kind == ContactDateKind.Birthday);

                var birthday = (b == null) ? default :
                    new DateTime((int)b?.Year, (int)b?.Month, (int)b?.Day, 0, 0, 0);

                return new Contact(
                                    phoneContact.Name,
                                    phones,
                                    emails,
                                    birthday,
                                    ContactType.Unknown);
            }
            catch (Exception)
            {
                throw;
            }
        }

        static ContactType GetPhoneContactType(ContactPhoneKind type)
        {
            switch (type)
            {
                case ContactPhoneKind.Home:
                case ContactPhoneKind.HomeFax:
                case ContactPhoneKind.Mobile:
                    return ContactType.Personal;
                case ContactPhoneKind.Work:
                case ContactPhoneKind.Pager:
                case ContactPhoneKind.BusinessFax:
                case ContactPhoneKind.Company:
                    return ContactType.Work;
                default:
                    return ContactType.Unknown;
            }
        }

        static ContactType GetEmailContactType(ContactEmailKind type) => type switch
        {
            ContactEmailKind.Personal => ContactType.Personal,
            ContactEmailKind.Work => ContactType.Work,
            _ => ContactType.Unknown,
        };
    }
}
