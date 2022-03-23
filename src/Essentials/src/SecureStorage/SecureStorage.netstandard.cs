using System.Threading.Tasks;

namespace Microsoft.Maui.Storage
{
	partial class SecureStorageImplementation : ISecureStorage
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
