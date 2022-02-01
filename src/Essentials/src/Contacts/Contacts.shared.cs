using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Contacts']/Docs" />
	public static partial class Contacts
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='PickContactAsync']/Docs" />
		public static async Task<Contact> PickContactAsync()
		{
			// iOS does not require permissions for the picker
			if (DeviceInfo.Platform != DevicePlatform.iOS)
				await Permissions.EnsureGrantedAsync<Permissions.ContactsRead>();

			return await PlatformPickContactAsync();
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='GetAllAsync']/Docs" />
		public static Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
			=> PlatformGetAllAsync(cancellationToken);
	}
}
