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

		public Entry? Write(Uri uri, byte[] data, string? eTag, string? lastModified, DateTimeOffset now)
		{
			if (!IsEnabled || data.LongLength > _sizeLimit)
				return null;

			var fileName = GetFileName(uri);
			var dataPath = Path.Combine(Directory!, fileName);

			lock (GetKeyLock(fileName))
			{
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

		bool IsCurrent(Entry entry)
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

		public void Remove(Uri uri)
		{
			if (!IsEnabled)
				return;

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
				}

				var dataPath = Path.Combine(Directory!, fileName);
				TryDelete(dataPath);
				TryDelete(dataPath + MetadataExtension);
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
		public void ResetIndex()
		{
			lock (_indexBuildLock)
			lock (_lock)
			{
				_index = new Dictionary<string, Entry>(StringComparer.Ordinal);
				_size = 0;
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

				foreach (var path in System.IO.Directory.EnumerateFiles(Directory))
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
			if (_acceptFilesWithoutMetadata ||
				path.EndsWith(MetadataExtension, StringComparison.Ordinal) ||
				path.EndsWith(TempExtension, StringComparison.Ordinal))
			{
				return true;
			}

			return File.Exists(path + MetadataExtension);
		}

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
							if (info.LastWriteTimeUtc < staleTempTime)
								TryDelete(info.FullName);
							continue;
						}

						if (name.EndsWith(MetadataExtension, StringComparison.Ordinal))
						{
							if (!File.Exists(info.FullName.Substring(0, info.FullName.Length - MetadataExtension.Length)))
								TryDelete(info.FullName);
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
