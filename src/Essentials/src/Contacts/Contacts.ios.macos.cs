using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contacts;
#if __IOS__
using ContactsUI;
#endif

namespace Microsoft.Maui.ApplicationModel.Communication
{
	class ContactsImplementation : IContacts
	{
#if __MACOS__
		public Task<Contact> PickContactAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
#elif __IOS__
		public Task<Contact> PickContactAsync()
		{
			var vc = WindowStateManager.Default.GetCurrentUIViewController(true);

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

			picker.PresentationController?.Delegate =
					new UIPresentationControllerDelegate(() => source?.TrySetResult(null));

			vc.PresentViewController(picker, true, null);

			return source.Task;
		}
#endif

		public Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken)
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
#pragma warning disable CA1416 // picker.DismissModalViewController(bool) has UnsupportedOSPlatform("ios6.0")]. (Deprecated but still works)
#pragma warning disable CA1422 // Validate platform compatibility
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

#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
		}
#endif
	}
}
