using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Contacts.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Contacts']/Docs" />
	public class ContactsImplementation : IContacts
	{
		public Task<Contact> PickContactAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken) => throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
