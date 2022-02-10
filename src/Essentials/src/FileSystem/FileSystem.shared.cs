using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileSystem']/Docs" />
	public static partial class FileSystem
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='CacheDirectory']/Docs" />
		public static string CacheDirectory
			=> PlatformCacheDirectory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='AppDataDirectory']/Docs" />
		public static string AppDataDirectory
			=> PlatformAppDataDirectory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileSystem.xml" path="//Member[@MemberName='OpenAppPackageFileAsync']/Docs" />
		public static Task<Stream> OpenAppPackageFileAsync(string filename)
			=> PlatformOpenAppPackageFileAsync(filename);

		internal static class MimeTypes
		{
			internal const string All = "*/*";

			internal const string ImageAll = "image/*";
			internal const string ImagePng = "image/png";
			internal const string ImageJpg = "image/jpeg";

			internal const string VideoAll = "video/*";

			internal const string EmailMessage = "message/rfc822";

			internal const string Pdf = "application/pdf";

			internal const string TextPlain = "text/plain";

			internal const string OctetStream = "application/octet-stream";
		}

		internal static class Extensions
		{
			internal const string Png = ".png";
			internal const string Jpg = ".jpg";
			internal const string Jpeg = ".jpeg";
			internal const string Gif = ".gif";
			internal const string Bmp = ".bmp";

			internal const string Avi = ".avi";
			internal const string Flv = ".flv";
			internal const string Gifv = ".gifv";
			internal const string Mp4 = ".mp4";
			internal const string M4v = ".m4v";
			internal const string Mpg = ".mpg";
			internal const string Mpeg = ".mpeg";
			internal const string Mp2 = ".mp2";
			internal const string Mkv = ".mkv";
			internal const string Mov = ".mov";
			internal const string Qt = ".qt";
			internal const string Wmv = ".wmv";

			internal const string Pdf = ".pdf";

			internal static string[] AllImage =>
				new[] { Png, Jpg, Jpeg, Gif, Bmp };

			internal static string[] AllJpeg =>
				new[] { Jpg, Jpeg };

			internal static string[] AllVideo =>
				new[] { Mp4, Mov, Avi, Wmv, M4v, Mpg, Mpeg, Mp2, Mkv, Flv, Gifv, Qt };

			internal static string Clean(string extension, bool trimLeadingPeriod = false)
			{
				if (string.IsNullOrWhiteSpace(extension))
					return string.Empty;

				extension = extension.TrimStart('*');
				extension = extension.TrimStart('.');

				if (!trimLeadingPeriod)
					extension = "." + extension;

				return extension;
			}
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileBase']/Docs" />
	public abstract partial class FileBase
	{
		internal const string DefaultContentType = FileSystem.MimeTypes.OctetStream;

		string contentType;

		// The caller must setup FullPath at least!!!
		internal FileBase()
		{
		}

		internal FileBase(string fullPath)
		{
			if (fullPath == null)
				throw new ArgumentNullException(nameof(fullPath));
			if (string.IsNullOrWhiteSpace(fullPath))
				throw new ArgumentException("The file path cannot be an empty string.", nameof(fullPath));
			if (string.IsNullOrWhiteSpace(Path.GetFileName(fullPath)))
				throw new ArgumentException("The file path must be a file path.", nameof(fullPath));

			FullPath = fullPath;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public FileBase(FileBase file)
		{
			FullPath = file.FullPath;
			ContentType = file.ContentType;
			FileName = file.FileName;
			PlatformInit(file);
		}

		internal FileBase(string fullPath, string contentType)
			: this(fullPath)
		{
			FullPath = fullPath;
			ContentType = contentType;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="//Member[@MemberName='FullPath']/Docs" />
		public string FullPath { get; internal set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="//Member[@MemberName='ContentType']/Docs" />
		public string ContentType
		{
			get => GetContentType();
			set => contentType = value;
		}

		internal string GetContentType()
		{
			// try the provided type
			if (!string.IsNullOrWhiteSpace(contentType))
				return contentType;

			// try get from the file extension
			var ext = Path.GetExtension(FullPath);
			if (!string.IsNullOrWhiteSpace(ext))
			{
				var content = PlatformGetContentType(ext);
				if (!string.IsNullOrWhiteSpace(content))
					return content;
			}

			return DefaultContentType;
		}

		string fileName;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="//Member[@MemberName='FileName']/Docs" />
		public string FileName
		{
			get => GetFileName();
			set => fileName = value;
		}

		internal string GetFileName()
		{
			// try the provided file name
			if (!string.IsNullOrWhiteSpace(fileName))
				return fileName;

			// try get from the path
			if (!string.IsNullOrWhiteSpace(FullPath))
				return Path.GetFileName(FullPath);

			// this should never happen as the path is validated in the constructor
			throw new InvalidOperationException($"Unable to determine the file name from '{FullPath}'.");
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="//Member[@MemberName='OpenReadAsync']/Docs" />
		public Task<Stream> OpenReadAsync()
			=> PlatformOpenReadAsync();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ReadOnlyFile.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ReadOnlyFile']/Docs" />
	public class ReadOnlyFile : FileBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ReadOnlyFile.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public ReadOnlyFile(string fullPath)
			: base(fullPath)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ReadOnlyFile.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ReadOnlyFile(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ReadOnlyFile.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public ReadOnlyFile(FileBase file)
			: base(file)
		{
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FileResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileResult']/Docs" />
	public partial class FileResult : FileBase
	{
		// The caller must setup FullPath at least!!!
		internal FileResult()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileResult.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public FileResult(string fullPath)
			: base(fullPath)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileResult.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public FileResult(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FileResult.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public FileResult(FileBase file)
			: base(file)
		{
		}
	}
}
