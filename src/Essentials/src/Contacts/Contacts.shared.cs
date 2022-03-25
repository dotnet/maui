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

	/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Contacts']/Docs" />
	public static class Contacts
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='PickContactAsync']/Docs" />
		public static Task<Contact?> PickContactAsync() =>
			Current.PickContactAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="//Member[@MemberName='GetAllAsync']/Docs" />
		public static Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default) =>
			Current.GetAllAsync(cancellationToken);

		static IContacts? currentImplementation;

		public static IContacts Current =>
			currentImplementation ??= new ContactsImplementation();

		internal static void SetCurrent(IContacts? implementation) =>
			currentImplementation = implementation;
	}
}
