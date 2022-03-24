#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Contacts']/Docs" />
	public static partial class Contacts
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='PickContactAsync']/Docs" />
		[Obsolete($"Use {nameof(Contacts)}.{nameof(Current)} instead.", true)]
		public static async Task<Contact?> PickContactAsync()
		{
			// iOS does not require permissions for the picker
			if (DeviceInfo.Current.Platform != DevicePlatform.iOS)
				await Permissions.EnsureGrantedAsync<Permissions.ContactsRead>();

			return await Current.PickContactAsync();
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='GetAllAsync']/Docs" />
		[Obsolete($"Use {nameof(Contacts)}.{nameof(Current)} instead.", true)]
		public static Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
			=> Current.GetAllAsync(cancellationToken);
	}
}
