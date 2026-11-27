#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Stores downloaded images on disk with the metadata needed to expire and revalidate them.
	/// </summary>
	/// <remarks>
	/// Each image is stored as <c>{crc64}{extension}</c> (the same name the iOS cache used before) next to a <c>.meta</c> file.
	/// Image files without a <c>.meta</c> file are treated as downloaded at their last write time.
	/// All I/O is best effort: failures never fail an image load.
	/// </remarks>
	internal sealed class UriImageDiskCache
	{
		const string MetadataExtension = ".meta";
		const string TempExtension = ".tmp";
		const string MetadataHeader = "maui-uri-image-v1";

		readonly object _lock = new();
		readonly object _indexBuildLock = new();
		readonly object[] _keyLocks = CreateKeyLocks();
		readonly bool _acceptFilesWithoutMetadata;
		readonly long _sizeLimit;
		readonly Func<DateTimeOffset> _utcNow;
		Dictionary<string, Entry>? _index;
		long _size;
		Task? _indexing;

		/// <param name="directory">The directory to store images in, or <see langword="null"/> to disable the disk cache.</param>
		/// <param name="sizeLimit">The maximum total size of the stored images.</param>
		/// <param name="utcNow">The clock.</param>
		/// <param name="acceptFilesWithoutMetadata">
		/// Whether image files without a <c>.meta</c> file are cached images (written by earlier versions in the default directory).
		/// In other directories such files are left alone.
		/// </param>
		public UriImageDiskCache(string? directory, long sizeLimit, Func<DateTimeOffset>? utcNow = null, bool acceptFilesWithoutMetadata = false)
		{
			Directory = string.IsNullOrEmpty(directory) ? null : directory;
			_sizeLimit = Math.Max(0, sizeLimit);
			_utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
			_acceptFilesWithoutMetadata = acceptFilesWithoutMetadata;
		}

		public string? Directory { get; }

		public bool IsEnabled => Directory is not null && _sizeLimit > 0;

		public long Size
		{
			get
			{
				lock (_lock)
					return _size;
			}
		}

		/// <summary>
		/// Whether <paramref name="fileName"/> is a file this cache writes: <c>{crc64}{extension}</c>, its <c>.meta</c> file, or a temporary file for either.
		/// </summary>
		/// <remarks>Other files in the directory are never indexed, evicted or deleted.</remarks>
		internal static bool IsCacheFileName(string fileName)
		{
			const int HashLength = 16;

			if (fileName.Length < HashLength)
				return false;

			for (var i = 0; i < HashLength; i++)
			{
				var c = fileName[i];
				if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')))
					return false;
			}

			return fileName.Length == HashLength || fileName[HashLength] == '.';
		}

		internal static string GetFileName(Uri uri)
		{
			var hash = Crc64.ComputeHashString(uri.OriginalString);
			string extension;
			try
			{
				extension = Path.GetExtension(uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString);
			}
			catch (ArgumentException)
			{
				extension = string.Empty;
			}
			if (extension.Length > 10 ||
				extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
				extension.Equals(MetadataExtension, StringComparison.OrdinalIgnoreCase) ||
				extension.Equals(TempExtension, StringComparison.OrdinalIgnoreCase))
			{
				// Keep image files distinguishable from the cache's own metadata and temporary files.
				extension = string.Empty;
			}
			return hash + extension;
		}

		/// <summary>
		/// Starts building the in-memory index in the background so that <see cref="TryGetEntry"/> can answer without I/O.
		/// </summary>
		public void BeginIndexing()
		{
			if (!IsEnabled)
				return;

			lock (_lock)
			{
				if (_index is not null || _indexing is not null)
					return;

				_indexing = Task.Run(EnsureIndex);
			}
		}

		/// <summary>
		/// Looks up an entry without performing any I/O. Returns <see langword="false"/> when the index is not built yet.
		/// </summary>
		public bool TryGetEntry(Uri uri, out Entry? entry)
		{
			entry = null;
			if (!IsEnabled)
				return false;

			lock (_lock)
			{
				if (_index is null)
					return false;

				entry = FindLocked(uri);
				if (entry is not null)
					entry.LastAccessUtcTicks = _utcNow().UtcTicks;
				return true;
			}
		}

		public Entry? GetEntry(Uri uri)
		{
			if (!IsEnabled)
				return null;

			EnsureIndex();

			lock (_lock)
			{
				var entry = FindLocked(uri);
				if (entry is not null)
					entry.LastAccessUtcTicks = _utcNow().UtcTicks;
				return entry;
			}
		}

		public async Task<byte[]?> TryReadAsync(Entry entry, CancellationToken cancellationToken)
		{
			try
			{
				// Allow the file to be replaced or deleted while it is read (Windows locks it otherwise).
				using var stream = new FileStream(entry.DataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096, useAsync: true);
				var buffer = new byte[stream.Length];
				var read = 0;
				while (read < buffer.Length)
				{
					var count = await stream.ReadAsync(buffer, read, buffer.Length - read, cancellationToken).ConfigureAwait(false);
					if (count == 0)
						break;
					read += count;
				}
				return read == buffer.Length ? buffer : null;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				Remove(entry);
				return null;
			}
		}

		/// <param name="uri">The URI of the image.</param>
		/// <param name="data">The image.</param>
		/// <param name="eTag">The ETag response header, if any.</param>
		/// <param name="lastModified">The Last-Modified response header, if any.</param>
		/// <param name="now">The current time.</param>
		/// <param name="canPublish">
		/// Checked before writing and again (atomically with <see cref="ResetIndex"/> and <see cref="Remove(Uri, Action?)"/>)
		/// before the entry is added; the entry is not stored when it returns <see langword="false"/>.
		/// </param>
		public Entry? Write(Uri uri, byte[] data, string? eTag, string? lastModified, DateTimeOffset now, Func<bool>? canPublish = null)
		{
			if (!IsEnabled || data.LongLength > _sizeLimit)
				return null;

			var fileName = GetFileName(uri);
			var dataPath = Path.Combine(Directory!, fileName);

			lock (GetKeyLock(fileName))
			{
				if (canPublish is not null && !canPublish())
					return null;

				try
				{
					System.IO.Directory.CreateDirectory(Directory!);

					var entry = new Entry(fileName, dataPath)
					{
						Uri = uri.OriginalString,
						Length = data.LongLength,
						DownloadedUtcTicks = now.UtcTicks,
						ValidatedUtcTicks = now.UtcTicks,
						LastAccessUtcTicks = now.UtcTicks,
						ETag = eTag,
						LastModified = lastModified,
					};

					WriteAtomically(dataPath, stream => stream.Write(data, 0, data.Length));
					try
					{
						WriteMetadata(entry);
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
					{
						// Never leave new image data next to the metadata (validators) of the previous download.
						TryDelete(dataPath);
						TryDelete(dataPath + MetadataExtension);
						lock (_lock)
						{
							if (_index is not null && _index.TryGetValue(fileName, out var previous))
							{
								_index.Remove(fileName);
								_size -= previous.Length;
							}
						}
						throw;
					}

					EnsureIndex();
					lock (_lock)
					{
						if (canPublish is not null && !canPublish())
						{
							// The cache was cleared (or the image removed) while it was written.
							TryDelete(dataPath);
							TryDelete(dataPath + MetadataExtension);
							return null;
						}

						if (_index!.TryGetValue(fileName, out var existing))
							_size -= existing.Length;
						_index[fileName] = entry;
						_size += entry.Length;
						EvictLocked(keep: fileName);
					}

					return entry;
				}
				catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Marks an entry as fresh again after the server confirmed (HTTP 304) that it did not change.
		/// </summary>
		public bool Renew(Entry entry, string? eTag, string? lastModified, DateTimeOffset now)
		{
			lock (GetKeyLock(entry.FileName))
			{
				if (!IsCurrent(entry) || !File.Exists(entry.DataPath))
				{
					Remove(entry);
					return false;
				}

				RenewLocked(entry, eTag, lastModified, now);
				return true;
			}
		}

		static void RenewLocked(Entry entry, string? eTag, string? lastModified, DateTimeOffset now)
		{
			lock (entry)
			{
				entry.ValidatedUtcTicks = now.UtcTicks;
				entry.LastAccessUtcTicks = now.UtcTicks;
				if (eTag is not null)
					entry.ETag = eTag;
				if (lastModified is not null)
					entry.LastModified = lastModified;
			}

			try
			{
				WriteMetadata(entry);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// The in-memory index is still renewed; the file is renewed again on the next validation.
			}
		}

		/// <summary>Whether <paramref name="entry"/> is still the cached version of its image.</summary>
		public bool IsCurrent(Entry entry)
		{
			lock (_lock)
				return _index is not null && _index.TryGetValue(entry.FileName, out var current) && ReferenceEquals(current, entry);
		}

		// A fixed set of locks, so that memory does not grow with the number of images ever cached.
		object GetKeyLock(string fileName) =>
			_keyLocks[(StringComparer.Ordinal.GetHashCode(fileName) & int.MaxValue) % _keyLocks.Length];

		static object[] CreateKeyLocks()
		{
			var locks = new object[64];
			for (var i = 0; i < locks.Length; i++)
				locks[i] = new object();
			return locks;
		}

		/// <summary>
		/// Removes the image of <paramref name="uri"/>.
		/// </summary>
		/// <param name="uri">The URI of the image.</param>
		/// <param name="whileLocked">Invoked atomically with removing the entry from the index (also when the disk cache is disabled).</param>
		public void Remove(Uri uri, Action? whileLocked = null)
		{
			if (!IsEnabled)
			{
				lock (_lock)
					whileLocked?.Invoke();
				return;
			}

			var fileName = GetFileName(uri);
			lock (GetKeyLock(fileName))
			{
				lock (_lock)
				{
					if (_index is not null && _index.TryGetValue(fileName, out var entry))
					{
						_index.Remove(fileName);
						_size -= entry.Length;
					}

					whileLocked?.Invoke();
				}

				var dataPath = Path.Combine(Directory!, fileName);
				TryDelete(dataPath);
				TryDelete(dataPath + MetadataExtension);
			}
		}

		/// <summary>
		/// Removes the image cached for <paramref name="uri"/>, if the cache has one. Unlike <see cref="Remove(Uri, Action?)"/>,
		/// files the cache does not know about are left alone.
		/// </summary>
		/// <param name="uri">The URI of the image.</param>
		/// <param name="whileLocked">Invoked atomically with the removal, also when nothing is cached.</param>
		public void RemoveIfCached(Uri uri, Action whileLocked)
		{
			if (!IsEnabled)
			{
				lock (_lock)
					whileLocked();
				return;
			}

			var fileName = GetFileName(uri);
			lock (GetKeyLock(fileName))
			{
				EnsureIndex();

				Entry? removed = null;
				lock (_lock)
				{
					if (_index!.TryGetValue(fileName, out var entry))
					{
						_index.Remove(fileName);
						_size -= entry.Length;
						removed = entry;
					}

					whileLocked();
				}

				if (removed is not null)
				{
					TryDelete(removed.DataPath);
					TryDelete(removed.DataPath + MetadataExtension);
				}
			}
		}

		/// <summary>
		/// Removes <paramref name="entry"/>, unless it was already replaced by a newer download of the same image.
		/// </summary>
		public void Remove(Entry entry)
		{
			lock (GetKeyLock(entry.FileName))
			{
				lock (_lock)
				{
					if (_index is not null && _index.TryGetValue(entry.FileName, out var existing))
					{
						if (existing != entry)
							return;

						_index.Remove(entry.FileName);
						_size -= entry.Length;
					}
				}

				TryDelete(entry.DataPath);
				TryDelete(entry.DataPath + MetadataExtension);
			}
		}

		/// <summary>
		/// Forgets all entries. Files are removed by <see cref="DeleteUnindexedFiles"/>.
		/// </summary>
		/// <param name="whileLocked">Invoked atomically with the reset.</param>
		public void ResetIndex(Action? whileLocked = null)
		{
			// An index build that is in progress does not replace this index (it only publishes when there is none),
			// so this does not need to wait for it.
			lock (_lock)
			{
				_index = new Dictionary<string, Entry>(StringComparer.Ordinal);
				_size = 0;
				whileLocked?.Invoke();
			}
		}

		/// <summary>
		/// Deletes the cache's files that are not (or no longer) part of the index, for example after <see cref="ResetIndex"/>.
		/// Images written after the reset are kept.
		/// </summary>
		public void DeleteUnindexedFiles()
		{
			if (Directory is null)
				return;

			try
			{
				if (!System.IO.Directory.Exists(Directory))
					return;

				// Image files first: whether an image file belongs to the cache depends on its metadata file,
				// which must therefore still exist when the image file is checked.
				var paths = System.IO.Directory.EnumerateFiles(Directory)
					.OrderBy(path => IsMetadataOrTempFile(path) ? 1 : 0)
					.ToList();

				foreach (var path in paths)
				{
					var name = Path.GetFileName(path);
					if (!IsCacheFileName(name) || !IsOwnedFile(path))
						continue;

					var fileName = GetDataFileName(name);
					lock (GetKeyLock(fileName))
					{
						bool indexed;
						lock (_lock)
							indexed = _index is not null && _index.ContainsKey(fileName);

						if (!indexed)
							TryDelete(path);
					}
				}
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Best effort.
			}
		}

		public void Clear()
		{
			ResetIndex();
			DeleteUnindexedFiles();
		}

		/// <summary>
		/// The image file name that <paramref name="name"/> (an image, metadata or temporary file) belongs to.
		/// </summary>
		static string GetDataFileName(string name)
		{
			if (name.EndsWith(TempExtension, StringComparison.Ordinal))
			{
				// {data}[.meta].{guid}.tmp
				var guidStart = name.Length - TempExtension.Length - 1;
				while (guidStart > 0 && name[guidStart] != '.')
					guidStart--;
				if (guidStart > 0)
					name = name.Substring(0, guidStart);
			}

			if (name.EndsWith(MetadataExtension, StringComparison.Ordinal))
				name = name.Substring(0, name.Length - MetadataExtension.Length);

			return name;
		}

		/// <summary>
		/// Whether a file with a cache file name was written by this cache: metadata and temporary files always are;
		/// image files are when they have metadata (or, in the default directory, also without it).
		/// </summary>
		bool IsOwnedFile(string path)
		{
			if (path.EndsWith(TempExtension, StringComparison.Ordinal))
				return IsTempFileName(Path.GetFileName(path));

			if (path.EndsWith(MetadataExtension, StringComparison.Ordinal))
				return IsMetadataFile(path);

			return _acceptFilesWithoutMetadata || IsMetadataFile(path + MetadataExtension);
		}

		/// <summary>Whether <paramref name="path"/> is a metadata file written by this cache (checked by its header).</summary>
		static bool IsMetadataFile(string path)
		{
			try
			{
				using var reader = new StreamReader(path);
				return reader.ReadLine() == MetadataHeader;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				return false;
			}
		}

		/// <summary>Whether <paramref name="fileName"/> has the form <c>{name}.{32 hex digits}.tmp</c> used by <see cref="WriteAtomically"/>.</summary>
		static bool IsTempFileName(string fileName)
		{
			const int GuidLength = 32;
			var guidEnd = fileName.Length - TempExtension.Length;
			var guidStart = guidEnd - GuidLength;
			if (guidStart < 1 || fileName[guidStart - 1] != '.')
				return false;

			for (var i = guidStart; i < guidEnd; i++)
			{
				var c = fileName[i];
				if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
					return false;
			}

			return true;
		}

		static bool IsMetadataOrTempFile(string path) =>
			path.EndsWith(MetadataExtension, StringComparison.Ordinal) || path.EndsWith(TempExtension, StringComparison.Ordinal);

		void EnsureIndex()
		{
			lock (_lock)
			{
				if (_index is not null)
					return;
			}

			// Only one thread scans the directory; the others wait for its result.
			lock (_indexBuildLock)
				BuildIndex();
		}

		void BuildIndex()
		{
			lock (_lock)
			{
				if (_index is not null)
					return;
			}

			var index = new Dictionary<string, Entry>(StringComparer.Ordinal);
			long size = 0;

			try
			{
				if (System.IO.Directory.Exists(Directory))
				{
					var staleTempTime = _utcNow().UtcDateTime.AddHours(-1);

					foreach (var info in new DirectoryInfo(Directory!).EnumerateFiles())
					{
						var name = info.Name;
						if (!IsCacheFileName(name))
							continue;

						if (name.EndsWith(TempExtension, StringComparison.Ordinal))
						{
							// Left behind by an interrupted write.
							if (info.LastWriteTimeUtc < staleTempTime)
								TryDeleteLeftover(info.FullName, () => true);
							continue;
						}

						if (name.EndsWith(MetadataExtension, StringComparison.Ordinal))
						{
							// Metadata whose image file is gone.
							var dataPath = info.FullName.Substring(0, info.FullName.Length - MetadataExtension.Length);
							TryDeleteLeftover(info.FullName, () => !File.Exists(dataPath));
							continue;
						}

						if (!IsOwnedFile(info.FullName))
							continue;

						var entry = new Entry(name, info.FullName)
						{
							Length = info.Length,
							DownloadedUtcTicks = info.LastWriteTimeUtc.Ticks,
							ValidatedUtcTicks = info.LastWriteTimeUtc.Ticks,
							LastAccessUtcTicks = info.LastWriteTimeUtc.Ticks,
						};
						ReadMetadata(entry);

						index[name] = entry;
						size += entry.Length;
					}
				}
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Start with whatever was indexed; the index is corrected as entries are written.
			}

			lock (_lock)
			{
				if (_index is not null)
					return;

				_index = index;
				_size = size;
				EvictLocked(keep: null);
			}
		}

		/// <summary>
		/// Deletes a leftover file found while indexing, re-checking <paramref name="isLeftover"/> while no write of that
		/// image can run (an image written in the meantime, for example after the cache was cleared, is kept).
		/// </summary>
		void TryDeleteLeftover(string path, Func<bool> isLeftover)
		{
			// Indexing holds the index build lock, which writers take while holding the image's lock: only try,
			// to never wait for a writer that waits for this scan. Skipped files are retried on the next start.
			var keyLock = GetKeyLock(GetDataFileName(Path.GetFileName(path)));
			if (!Monitor.TryEnter(keyLock))
				return;

			try
			{
				if (isLeftover() && IsOwnedFile(path))
					TryDelete(path);
			}
			finally
			{
				Monitor.Exit(keyLock);
			}
		}

		Entry? FindLocked(Uri uri)
		{
			var fileName = GetFileName(uri);
			if (!_index!.TryGetValue(fileName, out var entry))
				return null;

			// The file name is a 64-bit hash; the stored URI protects against collisions.
			if (entry.Uri is not null && !string.Equals(entry.Uri, uri.OriginalString, StringComparison.Ordinal))
				return null;

			return entry;
		}

		void EvictLocked(string? keep)
		{
			if (_size <= _sizeLimit)
				return;

			foreach (var entry in _index!.Values.OrderBy(e => e.LastAccessUtcTicks).ToList())
			{
				if (_size <= _sizeLimit)
					break;

				if (entry.FileName == keep)
					continue;

				// Skip images that are being written or renewed right now; they are evicted later if needed.
				var keyLock = GetKeyLock(entry.FileName);
				if (!Monitor.TryEnter(keyLock))
					continue;

				try
				{
					_index.Remove(entry.FileName);
					_size -= entry.Length;
					TryDelete(entry.DataPath);
					TryDelete(entry.DataPath + MetadataExtension);
				}
				finally
				{
					Monitor.Exit(keyLock);
				}
			}
		}

		static void ReadMetadata(Entry entry)
		{
			try
			{
				var path = entry.DataPath + MetadataExtension;
				if (!File.Exists(path))
					return;

				// Trailing empty values (no ETag or Last-Modified) are not returned as lines.
				var lines = File.ReadAllLines(path);
				if (lines.Length < 4 || lines[0] != MetadataHeader)
					return;

				entry.Uri = lines[1];
				if (long.TryParse(lines[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var downloaded))
					entry.DownloadedUtcTicks = downloaded;
				if (long.TryParse(lines[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var validated))
					entry.ValidatedUtcTicks = validated;
				entry.ETag = lines.Length > 4 && lines[4].Length > 0 ? lines[4] : null;
				entry.LastModified = lines.Length > 5 && lines[5].Length > 0 ? lines[5] : null;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Treat as an entry without metadata.
			}
		}

		static void WriteMetadata(Entry entry)
		{
			string[] lines;
			lock (entry)
			{
				lines = new[]
				{
					MetadataHeader,
					entry.Uri ?? string.Empty,
					entry.DownloadedUtcTicks.ToString(CultureInfo.InvariantCulture),
					entry.ValidatedUtcTicks.ToString(CultureInfo.InvariantCulture),
					Sanitize(entry.ETag),
					Sanitize(entry.LastModified),
				};
			}

			var text = string.Join("\n", lines);
			WriteAtomically(entry.DataPath + MetadataExtension, stream =>
			{
				var bytes = System.Text.Encoding.UTF8.GetBytes(text);
				stream.Write(bytes, 0, bytes.Length);
			});

			static string Sanitize(string? value) =>
				value is null ? string.Empty : new string(value.Where(c => c != '\r' && c != '\n').ToArray());
		}

		static void WriteAtomically(string path, Action<Stream> write)
		{
			var tempPath = path + "." + Guid.NewGuid().ToString("N") + TempExtension;
			try
			{
				using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
					write(stream);

#if NETSTANDARD
				if (File.Exists(path))
					File.Delete(path);
				File.Move(tempPath, path);
#else
				File.Move(tempPath, path, overwrite: true);
#endif
			}
			finally
			{
				TryDelete(tempPath);
			}
		}

		static void TryDelete(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Best effort.
			}
		}

		internal sealed class Entry
		{
			public Entry(string fileName, string dataPath)
			{
				FileName = fileName;
				DataPath = dataPath;
			}

			public string FileName { get; }

			public string DataPath { get; }

			/// <summary>The original URI, or <see langword="null"/> for files written before metadata was stored.</summary>
			public string? Uri { get; set; }

			public long Length { get; set; }

			// Read on other threads without locks, so updated atomically (also on 32-bit devices).
			long _downloadedUtcTicks;
			long _validatedUtcTicks;
			long _lastAccessUtcTicks;

			/// <summary>When the content was downloaded. Changes only when the content changes.</summary>
			public long DownloadedUtcTicks
			{
				get => Interlocked.Read(ref _downloadedUtcTicks);
				set => Interlocked.Exchange(ref _downloadedUtcTicks, value);
			}

			/// <summary>When the content was downloaded or last confirmed unchanged by the server.</summary>
			public long ValidatedUtcTicks
			{
				get => Interlocked.Read(ref _validatedUtcTicks);
				set => Interlocked.Exchange(ref _validatedUtcTicks, value);
			}

			public long LastAccessUtcTicks
			{
				get => Interlocked.Read(ref _lastAccessUtcTicks);
				set => Interlocked.Exchange(ref _lastAccessUtcTicks, value);
			}

			public string? ETag { get; set; }

			public string? LastModified { get; set; }
		}
	}
}
