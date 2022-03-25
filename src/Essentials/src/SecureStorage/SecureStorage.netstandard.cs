using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs" />
	public partial class SecureStorageImplementation : ISecureStorage
	{
		Task<string> PlatformGetAsync(string key) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformSetAsync(string key, string data) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		bool PlatformRemove(string key) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformRemoveAll() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
