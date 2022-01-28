using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Share']/Docs" />
	public static partial class Share
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][0]/Docs" />
		public static Task RequestAsync(string text) =>
			RequestAsync(new ShareTextRequest(text));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][4]/Docs" />
		public static Task RequestAsync(string text, string title) =>
			RequestAsync(new ShareTextRequest(text, title));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][3]/Docs" />
		public static Task RequestAsync(ShareTextRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Text) && string.IsNullOrEmpty(request.Uri))
				throw new ArgumentException($"Both the {nameof(request.Text)} and {nameof(request.Uri)} are invalid. Make sure to include at least one of them in the request.");

			return PlatformRequestAsync(request);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][1]/Docs" />
		public static Task RequestAsync(ShareFileRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (request.File == null)
				throw new ArgumentException(FileNullExeption(nameof(request.File)));

			return PlatformRequestAsync((ShareMultipleFilesRequest)request);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][2]/Docs" />
		public static Task RequestAsync(ShareMultipleFilesRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (!(request.Files?.Count() > 0))
				throw new ArgumentException(FileNullExeption(nameof(request.Files)));

			if (request.Files.Any(file => file == null))
				throw new ArgumentException(FileNullExeption(nameof(request.Files)));

			return PlatformRequestAsync(request);
		}

		static string FileNullExeption(string file)
			=> $"The {file} parameter in the request files is invalid";
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ShareRequestBase.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ShareRequestBase']/Docs" />
	public abstract class ShareRequestBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareRequestBase.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareRequestBase.xml" path="//Member[@MemberName='PresentationSourceBounds']/Docs" />
		public Rectangle PresentationSourceBounds { get; set; } = Rectangle.Zero;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ShareTextRequest']/Docs" />
	public class ShareTextRequest : ShareRequestBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public ShareTextRequest()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public ShareTextRequest(string text) => Text = text;

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ShareTextRequest(string text, string title)
			: this(text) => Title = title;

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="//Member[@MemberName='Subject']/Docs" />
		public string Subject { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareTextRequest.xml" path="//Member[@MemberName='Uri']/Docs" />
		public string Uri { get; set; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ShareFileRequest']/Docs" />
	public class ShareFileRequest : ShareRequestBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public ShareFileRequest()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="//Member[@MemberName='.ctor'][4]/Docs" />
		public ShareFileRequest(string title, ShareFile file)
		{
			Title = title;
			File = file;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public ShareFileRequest(string title, FileBase file)
		{
			Title = title;
			File = new ShareFile(file);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ShareFileRequest(ShareFile file)
			=> File = file;

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public ShareFileRequest(FileBase file)
			=> File = new ShareFile(file);

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFileRequest.xml" path="//Member[@MemberName='File']/Docs" />
		public ShareFile File { get; set; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ShareMultipleFilesRequest']/Docs" />
	public class ShareMultipleFilesRequest : ShareRequestBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public ShareMultipleFilesRequest()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ShareMultipleFilesRequest(IEnumerable<ShareFile> files) =>
			Files = files.ToList();

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ShareMultipleFilesRequest(IEnumerable<FileBase> files)
			: this(ConvertList(files))
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ShareMultipleFilesRequest(string title, IEnumerable<ShareFile> files)
			: this(files) => Title = title;

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ShareMultipleFilesRequest(string title, IEnumerable<FileBase> files)
			: this(title, ConvertList(files))
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareMultipleFilesRequest.xml" path="//Member[@MemberName='Files']/Docs" />
		public List<ShareFile> Files { get; set; }

		public static explicit operator ShareMultipleFilesRequest(ShareFileRequest request)
		{
			var requestFiles = new ShareMultipleFilesRequest(request.Title, new ShareFile[] { request.File });
			requestFiles.PresentationSourceBounds = request.PresentationSourceBounds;
			return requestFiles;
		}

		static IEnumerable<ShareFile> ConvertList(IEnumerable<FileBase> files)
			=> files?.Select(file => new ShareFile(file));
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFile.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ShareFile']/Docs" />
	public class ShareFile : FileBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFile.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public ShareFile(string fullPath)
			: base(fullPath)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFile.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ShareFile(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ShareFile.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public ShareFile(FileBase file)
			: base(file)
		{
		}
	}
}
