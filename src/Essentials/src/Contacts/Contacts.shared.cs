using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Contacts
	{
		public static async Task<Contact> PickContactAsync()
		{
			// iOS does not require permissions for the picker
			if (DeviceInfo.Platform != DevicePlatform.iOS)
				await Permissions.EnsureGrantedAsync<Permissions.ContactsRead>();

			return await PlatformPickContactAsync();
		}

		public static Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
			=> PlatformGetAllAsync(cancellationToken);
	}
}
