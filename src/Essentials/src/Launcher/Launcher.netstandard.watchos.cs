using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Launcher']/Docs" />
	public partial class LauncherImplementation
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
