using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FileSystemImplementation : IFileSystem
	/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileSystem']/Docs" />
	{
		public string CacheDirectory
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public string AppDataDirectory
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<Stream> OpenAppPackageFileAsync(string filename)
			 => throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<bool> PlatformAppPackageFileExistsAsync(string filename)
			 => throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileBase']/Docs" />
	public partial class FileBase
	{
		static string GetContentType(string extension) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal void Init(FileBase file) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal virtual Task<Stream> OpenReadAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
