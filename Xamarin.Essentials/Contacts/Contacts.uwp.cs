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
        static async Task<Contact> PlatformPickContactAsync()
        {
            var contactPicker = new ContactPicker();

            try
            {
                var contactSelected = await contactPicker.PickContactAsync();

                if (contactSelected == null)
                    return null;

                var phones = new List<ContactPhone>();
                var emails = new List<ContactEmail>();

                foreach (var item in contactSelected.Phones)
                    phones.Add(new ContactPhone(item.Number, GetPhoneContactType(item.Kind)));

                phones = phones.Distinct().ToList();

                foreach (var item in contactSelected.Emails)
                    emails.Add(new ContactEmail(item.Address, GetEmailContactType(item.Kind)));

                emails = emails.Distinct().ToList();

                return new Contact(
                                    contactSelected.Name,
                                    phones,
                                    emails,
                                    ContactType.Unknown);
            }
            catch (Exception)
            {
                throw;
            }
        }

        static ContactType GetPhoneContactType(ContactPhoneKind type)
            => type switch
            {
                ContactPhoneKind.Home => ContactType.Personal,
                ContactPhoneKind.HomeFax => ContactType.Personal,
                ContactPhoneKind.Mobile => ContactType.Personal,
                ContactPhoneKind.Work => ContactType.Work,
                ContactPhoneKind.Pager => ContactType.Work,
                ContactPhoneKind.BusinessFax => ContactType.Work,
                ContactPhoneKind.Company => ContactType.Work,
                _ => ContactType.Unknown
            };

        static ContactType GetEmailContactType(ContactEmailKind type) => type switch
        {
            ContactEmailKind.Personal => ContactType.Personal,
            ContactEmailKind.Work => ContactType.Work,
            _ => ContactType.Unknown,
        };
    }
}
