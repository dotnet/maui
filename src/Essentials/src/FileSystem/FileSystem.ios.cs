using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using Photos;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FileSystemImplementation : IFileSystem, IPlatformFileSystem
	{
		public async Task<FileResult[]> EnsurePhysicalFileResultsAsync(params NSUrl[] urls)
		{
			if (urls == null || urls.Length == 0)
				return Array.Empty<FileResult>();

			var opts = NSFileCoordinatorReadingOptions.WithoutChanges;
			var intents = urls.Select(x => NSFileAccessIntent.CreateReadingIntent(x, opts)).ToArray();

			using var coordinator = new NSFileCoordinator();

			var tcs = new TaskCompletionSource<FileResult[]>();

			coordinator.CoordinateAccess(intents, new NSOperationQueue(), error =>
			{
				if (error != null)
				{
					tcs.TrySetException(new NSErrorException(error));
					return;
				}

				var bookmarks = new List<FileResult>();

				foreach (var intent in intents)
				{
					var url = intent.Url;
					var result = new BookmarkDataFileResult(url);
					bookmarks.Add(result);
				}

				tcs.TrySetResult(bookmarks.ToArray());
			});

			return await tcs.Task;
		}
	}
}

namespace Microsoft.Maui.Essentials
{
	class BookmarkDataFileResult : FileResult
	{
		NSData bookmark;

		internal BookmarkDataFileResult(NSUrl url)
			: base()
		{
			try
			{
				url.StartAccessingSecurityScopedResource();

				var newBookmark = url.CreateBookmarkData(0, Array.Empty<string>(), null, out var bookmarkError);
				if (bookmarkError != null)
					throw new NSErrorException(bookmarkError);

				UpdateBookmark(url, newBookmark);
			}
			finally
			{
				url.StopAccessingSecurityScopedResource();
			}
		}

		void UpdateBookmark(NSUrl url, NSData newBookmark)
		{
			bookmark = newBookmark;

			var doc = new UIDocument(url);
			FullPath = doc.FileUrl?.Path;
			FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			var url = NSUrl.FromBookmarkData(bookmark, 0, null, out var isStale, out var error);

			if (error != null)
				throw new NSErrorException(error);

			url.StartAccessingSecurityScopedResource();

			if (isStale)
			{
				var newBookmark = url.CreateBookmarkData(NSUrlBookmarkCreationOptions.SuitableForBookmarkFile, Array.Empty<string>(), null, out error);
				if (error != null)
					throw new NSErrorException(error);

				UpdateBookmark(url, newBookmark);
			}

			var fileStream = File.OpenRead(FullPath);
			Stream stream = new SecurityScopedStream(fileStream, url);
			return Task.FromResult(stream);
		}

		class SecurityScopedStream : Stream
		{
			FileStream stream;
			NSUrl url;

			internal SecurityScopedStream(FileStream stream, NSUrl url)
			{
				this.stream = stream;
				this.url = url;
			}

			public override bool CanRead => stream.CanRead;

			public override bool CanSeek => stream.CanSeek;

			public override bool CanWrite => stream.CanWrite;

			public override long Length => stream.Length;

			public override long Position
			{
				get => stream.Position;
				set => stream.Position = value;
			}

			public override void Flush() =>
				stream.Flush();

			public override int Read(byte[] buffer, int offset, int count) =>
				stream.Read(buffer, offset, count);

			public override long Seek(long offset, SeekOrigin origin) =>
				stream.Seek(offset, origin);

			public override void SetLength(long value) =>
				stream.SetLength(value);

			public override void Write(byte[] buffer, int offset, int count) =>
				stream.Write(buffer, offset, count);

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if (disposing)
				{
					stream?.Dispose();
					stream = null;

					url?.StopAccessingSecurityScopedResource();
					url = null;
				}
			}
		}
	}

	class UIDocumentFileResult : FileResult
	{
		internal UIDocumentFileResult(NSUrl url)
			: base()
		{
			var doc = new UIDocument(url);
			FullPath = doc.FileUrl?.Path;
			FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			Stream fileStream = File.OpenRead(FullPath);

			return Task.FromResult(fileStream);
		}
	}

	class UIImageFileResult : FileResult
	{
		readonly UIImage uiImage;
		NSData data;

		internal UIImageFileResult(UIImage image)
			: base()
		{
			uiImage = image;

			FullPath = Guid.NewGuid().ToString() + FileSystem.Extensions.Png;
			FileName = FullPath;
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			data ??= uiImage.AsPNG();

			return Task.FromResult(data.AsStream());
		}
	}

	class PHAssetFileResult : FileResult
	{
		readonly PHAsset phAsset;

		internal PHAssetFileResult(NSUrl url, PHAsset asset, string originalFilename)
			: base()
		{
			phAsset = asset;

			FullPath = url?.AbsoluteString;
			FileName = originalFilename;
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			var tcsStream = new TaskCompletionSource<Stream>();

			System.Diagnostics.Debug.Assert(!OperatingSystem.IsIOSVersionAtLeast(13));
			PHImageManager.DefaultManager.RequestImageData(phAsset, null, new PHImageDataHandler((data, str, orientation, dict) =>
				tcsStream.TrySetResult(data.AsStream())));

			return tcsStream.Task;
		}
	}
}
