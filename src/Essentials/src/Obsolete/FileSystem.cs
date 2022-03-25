#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Storage
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileSystem']/Docs" />
	public static partial class FileSystem
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='CacheDirectory']/Docs" />
		[Obsolete($"Use {nameof(FileSystem)}.{nameof(Current)} instead.", true)]
		public static string CacheDirectory
			=> Current.CacheDirectory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='AppDataDirectory']/Docs" />
		[Obsolete($"Use {nameof(FileSystem)}.{nameof(Current)} instead.", true)]
		public static string AppDataDirectory
			=> Current.AppDataDirectory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='OpenAppPackageFileAsync']/Docs" />
		[Obsolete($"Use {nameof(FileSystem)}.{nameof(Current)} instead.", true)]
		public static Task<Stream> OpenAppPackageFileAsync(string filename)
			=> Current.OpenAppPackageFileAsync(filename);

		[Obsolete($"Use {nameof(FileSystem)}.{nameof(Current)} instead.", true)]
		public static Task<bool> AppPackageFileExistsAsync(string filename)
			=> Current.AppPackageFileExistsAsync(filename);
	}
}
