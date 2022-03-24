#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	public interface IContacts
	{
		Task<Contact?> PickContactAsync();

		Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default);
	}

	public static partial class Contacts
	{
		static IContacts? currentImplementation;

		public static IContacts Current =>
			currentImplementation ??= new ContactsImplementation();

		internal static void SetCurrent(IContacts? implementation) =>
			currentImplementation = implementation;
	}
}
