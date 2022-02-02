using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs" />
	public partial class SecureStorage
	{
		static Task<string> PlatformGetAsync(string key) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformSetAsync(string key, string data) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static bool PlatformRemove(string key) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformRemoveAll() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
