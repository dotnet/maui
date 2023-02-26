using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using ImageIO;
using UIKit;
using UTTypes = UniformTypeIdentifiers.UTTypes;
using OldUTType = MobileCoreServices.UTType;

namespace Microsoft.Maui.Storage
{
	static partial class FileSystemUtils
	{
		public static async Task<FileResult[]> EnsurePhysicalFileResultsAsync(params NSUrl[] urls)
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

	class UIDocumentFileResult : MediaFileResult
	{
		UIDocument document;
		NSUrl assetUrl;

		internal UIDocumentFileResult(NSUrl assetUrl, string fileName)
		{
			this.assetUrl = assetUrl;
			document = new UIDocument(assetUrl);
			Extension = document.FileUrl.PathExtension!;
			ContentType = GetMIMEType(document.FileType);
			NameWithoutExtension = !string.IsNullOrWhiteSpace(fileName)
				? Path.GetFileNameWithoutExtension(fileName)
				: null;
			Type = GetFileType(ContentType);
			FileName = GetFileName(NameWithoutExtension, Extension);
		}

		internal override Task<Stream> PlatformOpenReadAsync()
			=> Task.FromResult<Stream>(File.OpenRead(document.FileUrl.Path!));

		protected internal override void PlatformDispose()
		{
			document?.Dispose();
			document = null;
			assetUrl?.Dispose();
			assetUrl = null;
			base.PlatformDispose();
		}
	}

	class UIImageFileResult : MediaFileResult
	{
		UIImage img;
		NSDictionary metadata;
		NSMutableData imgWithMetadata;

		internal UIImageFileResult(UIImage img, NSDictionary metadata, string name)
		{
			this.img = img;
			this.metadata = metadata;
			NameWithoutExtension = name;
#pragma warning disable CA1422
			ContentType = GetMIMEType(OldUTType.JPEG);
			Extension = GetExtension(OldUTType.JPEG);
#pragma warning restore CA1422
			Type = GetFileType(ContentType);
			FileName = GetFileName(NameWithoutExtension, Extension);
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			imgWithMetadata ??= GetImageWithMeta();
			return Task.FromResult(imgWithMetadata?.AsStream());
		}

		public NSMutableData GetImageWithMeta()
		{
			if (img == null || metadata == null)
				return null;

			using var source = CGImageSource.FromData(img.AsJPEG());
			var destData = new NSMutableData();
			using var destination = CGImageDestination.Create(destData, source.TypeIdentifier, 1, null);
			destination.AddImage(source, 0, metadata);
			destination.Close();
			DisposeSources();
			return destData;
		}

		protected internal override void PlatformDispose()
		{
			imgWithMetadata?.Dispose();
			imgWithMetadata = null;
			DisposeSources();
			base.PlatformDispose();
		}

		void DisposeSources()
		{
			img?.Dispose();
			img = null;
			metadata?.Dispose();
			metadata = null;
		}
	}

	class PHPickerFileResult : MediaFileResult
	{
		readonly string identifier;
		NSItemProvider provider;

		internal PHPickerFileResult(NSItemProvider provider)
		{
			this.provider = provider;
			NameWithoutExtension = provider?.SuggestedName;

			identifier = GetIdentifier(provider?.RegisteredTypeIdentifiers);

			if (string.IsNullOrWhiteSpace(identifier))
				return;

			Extension = GetExtension(identifier);
			ContentType = GetMIMEType(identifier);
			Type = GetFileType(ContentType);
			FileName = GetFileName(NameWithoutExtension, Extension);
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
			=> (await provider?.LoadDataRepresentationAsync(identifier))?.AsStream();

		protected internal override void PlatformDispose()
		{
			provider?.Dispose();
			provider = null;
			base.PlatformDispose();
		}

		private string GetIdentifier(string[] identifiers)
		{
			if (!(identifiers?.Length > 0))
				return null;
		
			if (identifiers.Any(i => i.StartsWith(UTTypes.LivePhoto.Identifier)) && identifiers.Contains(UTTypes.Jpeg.Identifier))
				return identifiers.FirstOrDefault(i => i == UTTypes.Jpeg.Identifier);
			if (identifiers.Contains(UTTypes.QuickTimeMovie.Identifier))
				return identifiers.FirstOrDefault(i => i == UTTypes.QuickTimeMovie.Identifier);
			return identifiers.FirstOrDefault();
		}
	}
}
