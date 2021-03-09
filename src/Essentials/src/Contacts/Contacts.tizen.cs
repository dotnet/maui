using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Pims.Contacts;
using TizenContact = Tizen.Pims.Contacts.ContactsViews.Contact;
using TizenEmail = Tizen.Pims.Contacts.ContactsViews.Email;
using TizenName = Tizen.Pims.Contacts.ContactsViews.Name;
using TizenNumber = Tizen.Pims.Contacts.ContactsViews.Number;

namespace Microsoft.Maui.Essentials
{
	public static partial class Contacts
	{
		static ContactsManager manager = new ContactsManager();

		static async Task<Contact> PlatformPickContactAsync()
		{
			Permissions.EnsureDeclared<Permissions.ContactsRead>();
			Permissions.EnsureDeclared<Permissions.LaunchApp>();
			await Permissions.EnsureGrantedAsync<Permissions.ContactsRead>();

			var tcs = new TaskCompletionSource<Contact>();

			var appControl = new AppControl();
			appControl.Operation = AppControlOperations.Pick;
			appControl.ExtraData.Add(AppControlData.SectionMode, "single");
			appControl.LaunchMode = AppControlLaunchMode.Single;
			appControl.Mime = "application/vnd.tizen.contact";

			AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
			{
				Contact contact = null;

				if (result == AppControlReplyResult.Succeeded)
				{
					var contactId = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected)?.FirstOrDefault();

					if (int.TryParse(contactId, out var contactInt))
					{
						var record = manager.Database.Get(TizenContact.Uri, contactInt);
						if (record != null)
							contact = ToContact(record);
					}
				}
				tcs.TrySetResult(contact);
			});

			return await tcs.Task;
		}

		static Task<IEnumerable<Contact>> PlatformGetAllAsync(CancellationToken cancellationToken)
		{
			var contactsList = manager.Database.GetAll(TizenContact.Uri, 0, 0);

			return Task.FromResult(GetEnumerable());

			IEnumerable<Contact> GetEnumerable()
			{
				for (var i = 0; i < contactsList?.Count; i++)
				{
					yield return ToContact(contactsList.GetCurrentRecord());

					contactsList.MoveNext();
				}
			}
		}

		static Contact ToContact(ContactsRecord contactsRecord)
		{
			var record = contactsRecord.GetChildRecord(TizenContact.Name, 0);

			var phones = new List<ContactPhone>();
			var phonesCount = contactsRecord.GetChildRecordCount(TizenContact.Number);
			for (var i = 0; i < phonesCount; i++)
			{
				var nameRecord = contactsRecord.GetChildRecord(TizenContact.Number, i);
				var number = nameRecord.Get<string>(TizenNumber.NumberData);

				phones.Add(new ContactPhone(number));
			}

			var emails = new List<ContactEmail>();
			var emailCount = contactsRecord.GetChildRecordCount(TizenContact.Email);
			for (var i = 0; i < emailCount; i++)
			{
				var emailRecord = contactsRecord.GetChildRecord(TizenContact.Email, i);
				var addr = emailRecord.Get<string>(TizenEmail.Address);

				emails.Add(new ContactEmail(addr));
			}

			return new Contact(
				(record?.Get<int>(TizenName.ContactId))?.ToString(),
				record?.Get<string>(TizenName.Prefix),
				record?.Get<string>(TizenName.First),
				record?.Get<string>(TizenName.Addition),
				record?.Get<string>(TizenName.Last),
				record?.Get<string>(TizenName.Suffix),
				phones,
				emails);
		}
	}
}
