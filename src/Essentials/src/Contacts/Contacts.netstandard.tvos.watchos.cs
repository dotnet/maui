using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	class ContactsImplementation : IContacts
	{
		public Task<Contact> PickContactAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
