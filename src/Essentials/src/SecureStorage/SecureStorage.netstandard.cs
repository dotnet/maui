using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs" />
	public partial class SecureStorageImplementation : ISecureStorage
	{
		public Task<string> GetAsync(string key) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task SetAsync(string key, string data) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool Remove(string key) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void RemoveAll() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
