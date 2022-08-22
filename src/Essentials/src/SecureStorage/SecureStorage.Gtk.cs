using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
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
