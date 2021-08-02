using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Contacts
	{
		static Task<Contact> PlatformPickContactAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<IEnumerable<Contact>> PlatformGetAllAsync(CancellationToken cancellationToken) => throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
