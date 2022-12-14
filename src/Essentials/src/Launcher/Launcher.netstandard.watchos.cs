using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		Task<bool> PlatformCanOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<bool> PlatformOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<bool> PlatformOpenAsync(OpenFileRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<bool> PlatformTryOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
