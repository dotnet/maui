using System;
using System.IO;

namespace Microsoft.Maui.Resizetizer.Tests
{
	/// <summary>
	/// Creating symbolic links needs Developer Mode or elevation on Windows, so tests that need one
	/// have to be able to tell "not supported here" apart from a real failure.
	/// </summary>
	static class SymbolicLink
	{
		public static bool TryCreateDirectoryLink(string link, string target, out string error) =>
			TryCreate(() => Directory.CreateSymbolicLink(link, target), out error);

		public static bool TryCreateFileLink(string link, string target, out string error) =>
			TryCreate(() => File.CreateSymbolicLink(link, target), out error);

		static bool TryCreate(Action create, out string error)
		{
			try
			{
				create();
				error = null;
				return true;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
			{
				error = ex.Message;
				return false;
			}
		}
	}
}
