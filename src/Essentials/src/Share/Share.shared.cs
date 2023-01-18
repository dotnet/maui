#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	/// <summary>
	/// The Share API enables an application to share data such as text and web links to other applications on the device.
	/// </summary>
	public interface IShare
	{
		/// <summary>
		/// Show the operating systems user interface to share text.
		/// </summary>
		/// <param name="request">A <see cref="ShareTextRequest"/> object containing the details of the data to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task RequestAsync(ShareTextRequest request);

		/// <summary>
		/// Show the operating system's user interface to share a single file.
		/// </summary>
		/// <param name="request">A <see cref="ShareFileRequest"/> object containing the details of the data to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task RequestAsync(ShareFileRequest request);

		/// <summary>
		/// Show the operating system's user interface to share multiple files.
		/// </summary>
		/// <param name="request">A <see cref="ShareMultipleFilesRequest"/> object containing the details of the data to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task RequestAsync(ShareMultipleFilesRequest request);
	}

	/// <summary>
	/// The Share API enables an application to share data such as text and web links to other applications on the device.
	/// </summary>
	public static partial class Share
	{
		/// <summary>
		/// Show the operating system's user interface to share text.
		/// </summary>
		/// <param name="text">The text to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(string text) =>
			Current.RequestAsync(text);

		/// <summary>
		/// Show the operating system's user interface to share text.
		/// </summary>
		/// <param name="text">The text to share.</param>
		/// <param name="title">The title to display on the operating system share dialog.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(string text, string title) =>
			Current.RequestAsync(text, title);

		/// <summary>
		/// Show the operating system's user interface to share text.
		/// </summary>
		/// <param name="request">A <see cref="ShareTextRequest"/> object containing the details of the data to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(ShareTextRequest request) =>
			Current.RequestAsync(request);

		/// <summary>
		/// Show the operating system's user interface to share a single file.
		/// </summary>
		/// <param name="request">A <see cref="ShareFileRequest"/> object containing the details of the data to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(ShareFileRequest request) =>
			Current.RequestAsync(request);

		/// <summary>
		/// Show the operating system's user interface to share multiple files.
		/// </summary>
		/// <param name="request">A <see cref="ShareMultipleFilesRequest"/> object containing the details of the data to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(ShareMultipleFilesRequest request) =>
			Current.RequestAsync(request);

		static IShare Current => ApplicationModel.DataTransfer.Share.Default;

		static IShare? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IShare Default =>
			defaultImplementation ??= new ShareImplementation();

		internal static void SetDefault(IShare? implementation) =>
			defaultImplementation = implementation;
	}


	/// <summary>
	/// Concrete implementation of the <see cref="IShare"/> APIs.
	/// </summary>
	partial class ShareImplementation : IShare
	{
		/// <inheritdoc cref="IShare.RequestAsync(ShareTextRequest)"/>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <see cref="ShareTextRequest.Text"/> and <see cref="ShareTextRequest.Uri"/> are both <see langword="null"/> or empty.</exception>
		public Task RequestAsync(ShareTextRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Text) && string.IsNullOrEmpty(request.Uri))
				throw new ArgumentException($"Both the {nameof(request.Text)} and {nameof(request.Uri)} are invalid. Make sure to include at least one of them in the request.");

			return PlatformRequestAsync(request);
		}

		/// <inheritdoc cref="IShare.RequestAsync(ShareFileRequest)"/>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <see cref="ShareFileRequest.File"/> is <see langword="null"/>.</exception>
		public Task RequestAsync(ShareFileRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (request.File == null)
				throw new ArgumentException(FileNullException(nameof(request.File)));

			return PlatformRequestAsync(request);
		}

		/// <inheritdoc cref="IShare.RequestAsync(ShareMultipleFilesRequest)"/>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <see cref="ShareMultipleFilesRequest.Files"/> is <see langword="null"/> or empty or one of the entries in <see cref="ShareMultipleFilesRequest.Files"/> is <see langword="null"/>.</exception>
		public Task RequestAsync(ShareMultipleFilesRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (!(request.Files?.Count > 0))
				throw new ArgumentException(FileNullException(nameof(request.Files)));

			if (request.Files.Any(file => file == null))
				throw new ArgumentException(FileNullException(nameof(request.Files)));

			return PlatformRequestAsync(request);
		}

		static string FileNullException(string file)
			=> $"The {file} parameter in the request files is invalid";
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="IShare"/> APIs.
	/// </summary>
	public static class ShareExtensions
	{
		/// <summary>
		/// Show the operating system's user interface to share text.
		/// </summary>
		/// <param name="share">The object this method is invoked on.</param>
		/// <param name="text">The text to share.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(this IShare share, string text) =>
			share.RequestAsync(new ShareTextRequest(text));

		/// <summary>
		/// Show the operating systems user interface to share text.
		/// </summary>
		/// <param name="share">The object this method is invoked on.</param>
		/// <param name="text">The text to share.</param>
		/// <param name="title">The title to display on the operating system share dialog.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task RequestAsync(this IShare share, string text, string title) =>
			share.RequestAsync(new ShareTextRequest(text, title));
	}

	/// <summary>
	/// Base class for the different share requests that can be used with the <see cref="IShare"/> API.
	/// </summary>
	public abstract class ShareRequestBase
	{
		/// <summary>
		/// Gets or sets the title to use on the operating system's share user interface.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the source rectangle to display the iOS share user interface from.
		/// </summary>
		/// <remarks>This functionality is only available on iOS on an iPad.</remarks>
		public Rect PresentationSourceBounds { get; set; } = Rect.Zero;
	}

	/// <summary>
	/// Represents a request for sharing text with other apps on the device.
	/// </summary>
	public class ShareTextRequest : ShareRequestBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ShareTextRequest"/> class.
		/// </summary>
		public ShareTextRequest()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareTextRequest"/> class with the given text.
		/// </summary>
		/// <param name="text">The text to share.</param>
		public ShareTextRequest(string text) =>
			Text = text;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareTextRequest"/> class with the given text and title.
		/// </summary>
		/// <param name="text">The text to share.</param>
		/// <param name="title">The title to display on the operating systems share dialog.</param>
		public ShareTextRequest(string text, string title)
			: this(text) => Title = title;

		/// <summary>
		/// Gets or sets the main text or message to share.
		/// </summary>
		public string? Text { get; set; }

		/// <summary>
		/// Gets or sets the subject that is sometimes used for applications such as mail clients.
		/// </summary>
		/// <remarks>The subject is only applicable on Android and for certain apps.</remarks>
		public string? Subject { get; set; }

		/// <summary>
		/// Gets or sets a valid URI to share.
		/// </summary>
		/// <remarks>Needs to be a valid URI, else an exception may be thrown when sharing.</remarks>
		public string? Uri { get; set; }
	}

	/// <summary>
	/// Represents a request for sharing a single file with other apps on the device.
	/// </summary>
	public class ShareFileRequest : ShareRequestBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFileRequest"/> class.
		/// </summary>
		public ShareFileRequest()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFileRequest"/> class with the given title and file.
		/// </summary>
		/// <param name="title">The title to display on the operating systems share dialog.</param>
		/// <param name="file">The file to share.</param>
		public ShareFileRequest(string title, ShareFile file)
		{
			Title = title;
			File = file;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFileRequest"/> class with the given title and file.
		/// </summary>
		/// <param name="title">The title to display on the operating systems share dialog.</param>
		/// <param name="file">The file to share.</param>
		public ShareFileRequest(string title, FileBase file)
		{
			Title = title;
			File = new ShareFile(file);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFileRequest"/> class with the given file.
		/// </summary>
		/// <param name="file">The file to share.</param>
		public ShareFileRequest(ShareFile file)
			=> File = file;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFileRequest"/> class with the given file.
		/// </summary>
		/// <param name="file">The file to share.</param>
		public ShareFileRequest(FileBase file)
			=> File = new ShareFile(file);

		/// <summary>
		/// Gets or sets the file to share.
		/// </summary>
		public ShareFile? File { get; set; }
	}

	/// <summary>
	/// Represents a request for sharing multiple files with other apps on the device.
	/// </summary>
	public class ShareMultipleFilesRequest : ShareRequestBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ShareMultipleFilesRequest"/> class.
		/// </summary>
		public ShareMultipleFilesRequest()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareMultipleFilesRequest"/> class with the given files.
		/// </summary>
		/// <param name="files">A collection of files to share.</param>
		public ShareMultipleFilesRequest(IEnumerable<ShareFile> files) =>
			Files = files.ToList();

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareMultipleFilesRequest"/> class with the given files.
		/// </summary>
		/// <param name="files">A collection of files to share.</param>
		public ShareMultipleFilesRequest(IEnumerable<FileBase> files)
			: this(ConvertList(files))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareMultipleFilesRequest"/> class with the given title and files.
		/// </summary>
		/// <param name="title">The title to display on the operating systems share dialog.</param>
		/// <param name="files">A collection of files to share.</param>
		public ShareMultipleFilesRequest(string title, IEnumerable<ShareFile> files)
			: this(files) => Title = title;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareMultipleFilesRequest"/> class with the given title and files.
		/// </summary>
		/// <param name="title">The title to display on the operating systems share dialog.</param>
		/// <param name="files">A collection of files to share.</param>
		public ShareMultipleFilesRequest(string title, IEnumerable<FileBase> files)
			: this(title, ConvertList(files))
		{
		}

		/// <summary>
		/// Gets or sets the files to share.
		/// </summary>
		public List<ShareFile>? Files { get; set; } = new List<ShareFile>();

		/// <summary>
		/// Convert a single file share request into a multi-file share request.
		/// </summary>
		/// <param name="request">The <see cref="ShareFileRequest"/> object to convert into a <see cref="ShareMultipleFilesRequest"/> object.</param>
		public static explicit operator ShareMultipleFilesRequest(ShareFileRequest request) =>
			new ShareMultipleFilesRequest
			{
				Title = request.Title,
				Files = new List<ShareFile> { request.File! },
				PresentationSourceBounds = request.PresentationSourceBounds
			};

		static IEnumerable<ShareFile> ConvertList(IEnumerable<FileBase> files)
			=> files?.Select(file => new ShareFile(file)) ?? Array.Empty<ShareFile>();
	}

	/// <summary>
	/// A representation of a file that can be used with the <see cref="IShare"/> API.
	/// </summary>
	public class ShareFile : FileBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFile"/> class with the given file path.
		/// </summary>
		/// <param name="fullPath">The full path to the file represented by this object.</param>
		public ShareFile(string fullPath)
			: base(fullPath)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFile"/> class with the given file path and content type.
		/// </summary>
		/// <param name="fullPath">The full path to the file represented by this object.</param>
		/// <param name="contentType">Explicit content type (MIME type) of the file (eg: <c>image/png</c>).</param>
		public ShareFile(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShareFile"/> class from the given <see cref="FileBase"/> object.
		/// </summary>
		/// <param name="file">A <see cref="FileBase"/> object that will be wrapped in a <see cref="ShareFile"/> object.</param>
		public ShareFile(FileBase file)
			: base(file)
		{
		}
	}
}
