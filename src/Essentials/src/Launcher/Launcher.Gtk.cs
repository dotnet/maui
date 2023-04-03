using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		Task<bool> PlatformCanOpenAsync(Uri uri) => Task.FromResult(true);

		Task<bool> PlatformOpenAsync(Uri uri) => GTKTryOpenAsync(uri);

		Task<bool> PlatformOpenAsync(OpenFileRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<bool> PlatformTryOpenAsync(Uri uri) => GTKTryOpenAsync(uri);

		// ported from https://github.com/nblockchain/DotNetEssentials/blob/master/Xamarin.Essentials/Launcher/Launcher.netstandard.cs
		static async Task<bool> GTKTryOpenAsync(Uri uri)
		{
			string stdout, stderr;
			int exitCode;
			var task = Task.Run(
				() => GLib.Process.SpawnCommandLineSync("xdg-open " + uri.ToString(), out stdout, out stderr, out exitCode));
			var result = await task;
			return result;
		}
	}
}