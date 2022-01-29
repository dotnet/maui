using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Launcher']/Docs" />
	public static partial class Launcher
	{
		static Task<bool> PlatformCanOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformOpenAsync(OpenFileRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<bool> PlatformTryOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
