using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileSystem']/Docs" />
	public static partial class FileSystem
	{
		static string PlatformCacheDirectory
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static string PlatformAppDataDirectory
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
			 => throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileBase']/Docs" />
	public partial class FileBase
	{
		static string PlatformGetContentType(string extension) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal void PlatformInit(FileBase file) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal virtual Task<Stream> PlatformOpenReadAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
