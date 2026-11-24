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
		readonly long _sizeLimit;
		readonly Func<DateTimeOffset> _utcNow;
		Dictionary<string, Entry>? _index;
		long _size;
		Task? _indexing;

		public UriImageDiskCache(string? directory, long sizeLimit, Func<DateTimeOffset>? utcNow = null)
		{
			Directory = string.IsNullOrEmpty(directory) ? null : directory;
			_sizeLimit = Math.Max(0, sizeLimit);
			_utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
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

		internal static string GetFileName(Uri uri)
		{
			var hash = Crc64.ComputeHashString(uri.OriginalString);
			var extension = Path.GetExtension(uri.AbsolutePath);
			if (extension.Length > 10 || extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				extension = string.Empty;
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
#if NETSTANDARD
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
#else
				return await File.ReadAllBytesAsync(entry.DataPath, cancellationToken).ConfigureAwait(false);
#endif
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
				WriteMetadata(entry);

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

		/// <summary>
		/// Marks an entry as fresh again after the server confirmed (HTTP 304) that it did not change.
		/// </summary>
		public bool Renew(Entry entry, string? eTag, string? lastModified, DateTimeOffset now)
		{
			if (!File.Exists(entry.DataPath))
			{
				Remove(entry);
				return false;
			}

			lock (_lock)
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

			return true;
		}

		public void Remove(Uri uri)
		{
			if (!IsEnabled)
				return;

			var fileName = GetFileName(uri);
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

		public void Remove(Entry entry)
		{
			lock (_lock)
			{
				if (_index is not null && _index.TryGetValue(entry.FileName, out var existing) && existing == entry)
				{
					_index.Remove(entry.FileName);
					_size -= entry.Length;
				}
			}

			TryDelete(entry.DataPath);
			TryDelete(entry.DataPath + MetadataExtension);
		}

		public void Clear()
		{
			if (Directory is null)
				return;

			lock (_lock)
			{
				_index = new Dictionary<string, Entry>(StringComparer.Ordinal);
				_size = 0;
			}

			try
			{
				if (System.IO.Directory.Exists(Directory))
				{
					foreach (var file in System.IO.Directory.EnumerateFiles(Directory))
						TryDelete(file);
				}
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Best effort.
			}
		}

		void EnsureIndex()
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

				_index.Remove(entry.FileName);
				_size -= entry.Length;
				TryDelete(entry.DataPath);
				TryDelete(entry.DataPath + MetadataExtension);
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

			/// <summary>When the content was downloaded. Changes only when the content changes.</summary>
			public long DownloadedUtcTicks { get; set; }

			/// <summary>When the content was downloaded or last confirmed unchanged by the server.</summary>
			public long ValidatedUtcTicks { get; set; }

			public long LastAccessUtcTicks { get; set; }

			public string? ETag { get; set; }

			public string? LastModified { get; set; }
		}
	}
}
