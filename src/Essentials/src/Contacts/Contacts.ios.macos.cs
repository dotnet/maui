using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contacts;
#if __IOS__
using ContactsUI;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Contacts
	{
#if __MACOS__
        static Task<Contact> PlatformPickContactAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;

#elif __IOS__
        static Task<Contact> PlatformPickContactAsync()
        {
            var uiView = Platform.GetCurrentViewController();
            if (uiView == null)
                throw new ArgumentNullException($"The View Controller can't be null.");

            var source = new TaskCompletionSource<Contact>();

            var picker = new CNContactPickerViewController
            {
                Delegate = new ContactPickerDelegate(phoneContact =>
                {
                    try
                    {
                        source?.TrySetResult(ConvertContact(phoneContact));
                    }
                    catch (Exception ex)
                    {
                        source?.TrySetException(ex);
                    }
                })
            };

            uiView.PresentViewController(picker, true, null);

            return source.Task;
        }

#endif
		static Task<IEnumerable<Contact>> PlatformGetAllAsync(CancellationToken cancellationToken)
		{
			var keys = new[]
			{
				CNContactKey.Identifier,
				CNContactKey.NamePrefix,
				CNContactKey.GivenName,
				CNContactKey.MiddleName,
				CNContactKey.FamilyName,
				CNContactKey.NameSuffix,
				CNContactKey.EmailAddresses,
				CNContactKey.PhoneNumbers,
				CNContactKey.Type
			};

			var store = new CNContactStore();
			var containers = store.GetContainers(null, out _);
			if (containers == null)
				return Task.FromResult<IEnumerable<Contact>>(Array.Empty<Contact>());

			return Task.FromResult(GetEnumerable());

			IEnumerable<Contact> GetEnumerable()
			{
				foreach (var container in containers)
				{
					using var pred = CNContact.GetPredicateForContactsInContainer(container.Identifier);
					var contacts = store.GetUnifiedContacts(pred, keys, out var error);
					if (contacts == null)
						continue;

					foreach (var contact in contacts)
					{
						yield return ConvertContact(contact);
					}
				}
			}
		}

		internal static Contact ConvertContact(CNContact contact)
		{
			if (contact == null)
				return default;

			var phones = contact.PhoneNumbers?.Select(
				item => new ContactPhone(item?.Value?.StringValue));
			var emails = contact.EmailAddresses?.Select(
				item => new ContactEmail(item?.Value?.ToString()));

			return new Contact(
				contact.Identifier,
				contact.NamePrefix,
				contact.GivenName,
				contact.MiddleName,
				contact.FamilyName,
				contact.NameSuffix,
				phones,
				emails);
		}

#if __IOS__
        class ContactPickerDelegate : CNContactPickerDelegate
        {
            public ContactPickerDelegate(Action<CNContact> didSelectContactHandler) =>
                DidSelectContactHandler = didSelectContactHandler;

            public ContactPickerDelegate(IntPtr handle)
                : base(handle)
            {
            }

            public Action<CNContact> DidSelectContactHandler { get; }

            public override void ContactPickerDidCancel(CNContactPickerViewController picker)
            {
                DidSelectContactHandler?.Invoke(default);
                picker.DismissModalViewController(true);
            }

            public override void DidSelectContact(CNContactPickerViewController picker, CNContact contact)
            {
                DidSelectContactHandler?.Invoke(contact);
                picker.DismissModalViewController(true);
            }

            public override void DidSelectContactProperty(CNContactPickerViewController picker, CNContactProperty contactProperty) =>
                picker.DismissModalViewController(true);
        }
#endif
	}
}
