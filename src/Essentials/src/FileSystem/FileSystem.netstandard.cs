using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileSystem']/Docs" />
	partial class FileSystemImplementation : IFileSystem
	{
		string PlatformCacheDirectory
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		string PlatformAppDataDirectory
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<bool> PlatformAppPackageFileExistsAsync(string filename)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileBase']/Docs" />
	public partial class FileBase
	{
		static string PlatformGetContentType(string extension) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal void Init(FileBase file) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal virtual Task<Stream> PlatformOpenReadAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformInit(FileBase file)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
