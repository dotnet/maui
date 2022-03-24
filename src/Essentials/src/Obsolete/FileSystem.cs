#nullable enable
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileSystem']/Docs" />
	public static class FileSystem
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='CacheDirectory']/Docs" />
		public static string CacheDirectory
			=> Current.CacheDirectory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='AppDataDirectory']/Docs" />
		public static string AppDataDirectory
			=> Current.AppDataDirectory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='OpenAppPackageFileAsync']/Docs" />
		public static Task<Stream> OpenAppPackageFileAsync(string filename)
			=> Current.OpenAppPackageFileAsync(filename);

		public static Task<bool> AppPackageFileExistsAsync(string filename)
			=> Current.AppPackageFileExistsAsync(filename);

		static IFileSystem Current => Storage.FileSystem.Current;
	}
}
