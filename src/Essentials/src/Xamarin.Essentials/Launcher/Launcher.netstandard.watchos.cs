using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
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
