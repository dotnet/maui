#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IContacts
	{
		Task<Contact> PickContactAsync();
		Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default);
	}
	/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Contacts']/Docs" />
	public static class Contacts
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='PickContactAsync']/Docs" />
		public static async Task<Contact> PickContactAsync()
		{
			// iOS does not require permissions for the picker
			if (DeviceInfo.Platform != DevicePlatform.iOS)
				await Permissions.EnsureGrantedAsync<Permissions.ContactsRead>();

			return await Current.PickContactAsync();
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='GetAllAsync']/Docs" />
		public static Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
			=> Current.GetAllAsync(cancellationToken);

		static IContacts? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IContacts Current =>
			currentImplementation ??= new ContactsImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IContacts? implementation) =>
			currentImplementation = implementation;
	}
}
