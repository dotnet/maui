using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace Microsoft.Maui.Essentials
{
    public static partial class Contacts
    {
        static async Task<Contact> PlatformPickContactAsync()
        {
            var contactPicker = new ContactPicker();
            var contactSelected = await contactPicker.PickContactAsync();
            return ConvertContact(contactSelected);
        }

        static async Task<IEnumerable<Contact>> PlatformGetAllAsync(CancellationToken cancellationToken)
        {
            var contactStore = await ContactManager.RequestStoreAsync()
                .AsTask(cancellationToken).ConfigureAwait(false);
            if (contactStore == null)
                throw new PermissionException("Permission to access the contacts was denied.");

            var contacts = await contactStore.FindContactsAsync()
                .AsTask(cancellationToken).ConfigureAwait(false);
            if (contacts == null || contacts.Count == 0)
                return Array.Empty<Contact>();

            return GetEnumerable();

            IEnumerable<Contact> GetEnumerable()
            {
                foreach (var item in contacts)
                {
                    yield return ConvertContact(item);
                }
            }
        }

        internal static Contact ConvertContact(Windows.ApplicationModel.Contacts.Contact contact)
        {
            if (contact == null)
                return default;

            var phones = contact.Phones?.Select(
                item => new ContactPhone(item?.Number))?.ToList();
            var emails = contact.Emails?.Select(
                item => new ContactEmail(item?.Address))?.ToList();

            return new Contact(
                contact.Id,
                contact.HonorificNamePrefix,
                contact.FirstName,
                contact.MiddleName,
                contact.LastName,
                contact.HonorificNameSuffix,
                phones,
                emails,
                contact.DisplayName);
        }
    }
}
