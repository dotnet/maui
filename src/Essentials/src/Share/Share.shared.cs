using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if !NETSTANDARD1_0
using System.Drawing;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Share
	{
		public static Task RequestAsync(string text) =>
			RequestAsync(new ShareTextRequest(text));

		public static Task RequestAsync(string text, string title) =>
			RequestAsync(new ShareTextRequest(text, title));

		public static Task RequestAsync(ShareTextRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Text) && string.IsNullOrEmpty(request.Uri))
				throw new ArgumentException($"Both the {nameof(request.Text)} and {nameof(request.Uri)} are invalid. Make sure to include at least one of them in the request.");

			return PlatformRequestAsync(request);
		}

		public static Task RequestAsync(ShareFileRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (request.File == null)
				throw new ArgumentException(FileNullExeption(nameof(request.File)));

			return PlatformRequestAsync((ShareMultipleFilesRequest)request);
		}

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

	public abstract class ShareRequestBase
	{
		public string Title { get; set; }

#if !NETSTANDARD1_0
        public Rectangle PresentationSourceBounds { get; set; } = Rectangle.Empty;
#endif
	}

	public class ShareTextRequest : ShareRequestBase
	{
		public ShareTextRequest()
		{
		}

		public ShareTextRequest(string text) => Text = text;

		public ShareTextRequest(string text, string title)
			: this(text) => Title = title;

		public string Subject { get; set; }

		public string Text { get; set; }

		public string Uri { get; set; }
	}

	public class ShareFileRequest : ShareRequestBase
	{
		public ShareFileRequest()
		{
		}

		public ShareFileRequest(string title, ShareFile file)
		{
			Title = title;
			File = file;
		}

		public ShareFileRequest(string title, FileBase file)
		{
			Title = title;
			File = new ShareFile(file);
		}

		public ShareFileRequest(ShareFile file)
			=> File = file;

		public ShareFileRequest(FileBase file)
			=> File = new ShareFile(file);

		public ShareFile File { get; set; }
	}

	public class ShareMultipleFilesRequest : ShareRequestBase
	{
		public ShareMultipleFilesRequest()
		{
		}

		public ShareMultipleFilesRequest(IEnumerable<ShareFile> files) =>
			Files = files.ToList();

		public ShareMultipleFilesRequest(IEnumerable<FileBase> files)
			: this(ConvertList(files))
		{
		}

		public ShareMultipleFilesRequest(string title, IEnumerable<ShareFile> files)
			: this(files) => Title = title;

		public ShareMultipleFilesRequest(string title, IEnumerable<FileBase> files)
			: this(title, ConvertList(files))
		{
		}

		public List<ShareFile> Files { get; set; }

		public static explicit operator ShareMultipleFilesRequest(ShareFileRequest request)
		{
			var requestFiles = new ShareMultipleFilesRequest(request.Title, new ShareFile[] { request.File });
#if !NETSTANDARD1_0
            requestFiles.PresentationSourceBounds = request.PresentationSourceBounds;
#endif
			return requestFiles;
		}

		static IEnumerable<ShareFile> ConvertList(IEnumerable<FileBase> files)
			=> files?.Select(file => new ShareFile(file));
	}

	public class ShareFile : FileBase
	{
		public ShareFile(string fullPath)
			: base(fullPath)
		{
		}

		public ShareFile(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		public ShareFile(FileBase file)
			: base(file)
		{
		}
	}
}
