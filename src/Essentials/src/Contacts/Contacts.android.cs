using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;
using CommonDataKinds = Android.Provider.ContactsContract.CommonDataKinds;
using StructuredName = Android.Provider.ContactsContract.CommonDataKinds.StructuredName;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	class ContactsImplementation : IContacts
	{
		const string idCol = ContactsContract.Contacts.InterfaceConsts.Id;
		const string displayNameCol = ContactsContract.Contacts.InterfaceConsts.DisplayName;
		const string mimetypeCol = ContactsContract.Data.InterfaceConsts.Mimetype;

		const string contactIdCol = CommonDataKinds.Phone.InterfaceConsts.ContactId;

		public async Task<Contact> PickContactAsync()
		{
			var intent = new Intent(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
			var result = await IntermediateActivity.StartAsync(intent, PlatformUtils.requestCodePickContact).ConfigureAwait(false);
			if (result?.Data == null)
				return null;

			using var cursor = Application.Context.ContentResolver.Query(result?.Data, null, null, null, null);
			if (cursor?.MoveToFirst() != true)
				return null;

			return GetContact(cursor);
		}

		public Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken)
		{
			var cursor = Application.Context.ContentResolver.Query(ContactsContract.Contacts.ContentUri, null, null, null, null);
			return Task.FromResult(GetEnumerable());

			IEnumerable<Contact> GetEnumerable()
			{
				if (cursor?.MoveToFirst() == true)
				{
					do
					{
						var contact = GetContact(cursor);
						if (contact != null)
							yield return contact;
					}
					while (cursor.MoveToNext());
				}

				cursor?.Close();
			}
		}

		static Contact GetContact(ICursor cursor)
		{
			var id = GetString(cursor, idCol);
			var displayName = GetString(cursor, displayNameCol);
			var phones = GetNumbers(id)?.Select(p => new ContactPhone(p));
			var emails = GetEmails(id)?.Select(e => new ContactEmail(e));
			var (prefix, given, middle, family, suffix) = GetName(id);

			return new Contact(id, prefix, given, middle, family, suffix, phones, emails, displayName);
		}

		static IEnumerable<string> GetNumbers(string id)
		{
			var uri = CommonDataKinds.Phone.ContentUri
				.BuildUpon()
				.AppendQueryParameter(ContactsContract.RemoveDuplicateEntries, "1")
				.Build();

			var cursor = Application.Context.ContentResolver.Query(uri, null, $"{contactIdCol}=?", new[] { id }, null);

			return ReadCursorItems(cursor, CommonDataKinds.Phone.Number);
		}

		static IEnumerable<string> GetEmails(string id)
		{
			var uri = CommonDataKinds.Email.ContentUri
				.BuildUpon()
				.AppendQueryParameter(ContactsContract.RemoveDuplicateEntries, "1")
				.Build();

			var cursor = Application.Context.ContentResolver.Query(uri, null, $"{contactIdCol}=?", new[] { id }, null);

			return ReadCursorItems(cursor, CommonDataKinds.Email.Address);
		}

		static IEnumerable<string> ReadCursorItems(ICursor cursor, string dataKey)
		{
			if (cursor?.MoveToFirst() == true)
			{
				do
				{
					var data = GetString(cursor, dataKey);
					if (data != null)
						yield return data;
				}
				while (cursor.MoveToNext());
			}
			cursor?.Close();
		}

		static (string Prefix, string Given, string Middle, string Family, string Suffix) GetName(string id)
		{
			var selection = $"{mimetypeCol}=? AND {contactIdCol}=?";
			var selectionArgs = new string[] { StructuredName.ContentItemType, id };

			using var cursor = Application.Context.ContentResolver.Query(
				ContactsContract.Data.ContentUri,
				null,
				selection,
				selectionArgs,
				null);

			if (cursor?.MoveToFirst() != true)
				return (null, null, null, null, null);

			var result = (
				GetString(cursor, StructuredName.Prefix),
				GetString(cursor, StructuredName.GivenName),
				GetString(cursor, StructuredName.MiddleName),
				GetString(cursor, StructuredName.FamilyName),
				GetString(cursor, StructuredName.Suffix));

			cursor?.Close();

			return result;
		}

		static string GetString(ICursor cursor, string column) =>
			cursor.GetString(cursor.GetColumnIndex(column));
	}
}
