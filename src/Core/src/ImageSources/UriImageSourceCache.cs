#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui
{
	internal static class UriImageSourceCache
	{
		const string CacheFolderName = "MauiUriImages";
		internal const long MaxCacheSize = 100 * 1024 * 1024;
		internal const long MaxCacheEntrySize = 25 * 1024 * 1024;
		internal const long MaxTemporaryCacheSize = 25 * 1024 * 1024;
		static readonly object s_locksSync = new();
		static readonly Dictionary<string, CacheLock> s_locks = new();
		static readonly object s_trimSync = new();
		static readonly object s_temporaryCacheSync = new();
		static readonly HashSet<string> s_trimmedDirectories = new(StringComparer.Ordinal);
		static long s_temporaryCacheSize;

		public static Task<Stream?> GetStreamAsync(
			Uri uri,
			TimeSpan cacheValidity,
			Func<CancellationToken, Task<Stream?>> download,
			Action<Exception> cacheError,
			CancellationToken cancellationToken) =>
			GetStreamAsync(uri, cacheValidity, download, FileSystem.CacheDirectory, cancellationToken, cacheError);

		internal static async Task<Stream?> GetStreamAsync(
			Uri uri,
			TimeSpan cacheValidity,
			Func<CancellationToken, Task<Stream?>> download,
			string cacheDirectory,
			CancellationToken cancellationToken,
			Action<Exception>? cacheError = null,
			long maxCacheEntrySize = MaxCacheEntrySize)
		{
			if (cacheValidity <= TimeSpan.Zero)
				return await download(cancellationToken).ConfigureAwait(false);

			var cachePath = GetCachePath(uri, cacheDirectory);
			TrimCacheOnce(cacheDirectory, cacheError);

			using var cacheLock = await AcquireLockAsync(cachePath, cancellationToken).ConfigureAwait(false);

			var cached = TryOpenCachedFile(cachePath, cacheValidity, cacheError);
			if (cached is not null)
				return cached;

			string? temporaryPath;
			try
			{
				var directory = Path.GetDirectoryName(cachePath)!;
				Directory.CreateDirectory(directory);
				temporaryPath = Path.Combine(directory, $".{Path.GetFileName(cachePath)}.{Guid.NewGuid():N}.tmp");
			}
			catch (IOException ex)
			{
				cacheError?.Invoke(ex);
				return await download(cancellationToken).ConfigureAwait(false);
			}
			catch (UnauthorizedAccessException ex)
			{
				cacheError?.Invoke(ex);
				return await download(cancellationToken).ConfigureAwait(false);
			}

			var downloaded = await download(cancellationToken).ConfigureAwait(false);
			if (downloaded is null || !downloaded.CanRead)
			{
				downloaded?.Dispose();
				return null;
			}

			if (downloaded is IImageSourceCacheStream { CanCache: false })
				return downloaded;

			if (downloaded is IImageSourceCacheStream { ExpectedLength: long contentLength } &&
				contentLength > maxCacheEntrySize)
			{
				return downloaded;
			}

			var cacheFileCreated = false;
			try
			{
				TemporaryCacheReservation? reservation = new();
				try
				{
					CopyResult copyResult;
					using (var cacheStream = File.Create(temporaryPath))
					{
						cacheFileCreated = true;
						copyResult = await CopyToCacheAsync(
							downloaded,
							cacheStream,
							maxCacheEntrySize,
							reservation,
							cancellationToken).ConfigureAwait(false);
					}

					if (copyResult.Overflow is not null)
					{
						var temporaryStream = new TemporaryAndNetworkStream(
							temporaryPath,
							copyResult.Overflow,
							downloaded,
							reservation);
						temporaryPath = null;
						reservation = null;
						return temporaryStream;
					}

					downloaded.Dispose();
					downloaded = null;
					reservation.Dispose();
					reservation = null;

					var completedStream = OpenCacheFile(temporaryPath);
					try
					{
						if (File.Exists(cachePath))
							File.Replace(temporaryPath, cachePath, null);
						else
							File.Move(temporaryPath, cachePath);
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
					{
						cacheError?.Invoke(ex);
						var temporaryStream = new TemporaryFileStream(completedStream, temporaryPath);
						temporaryPath = null;
						return temporaryStream;
					}

					TrimCache(cacheDirectory, MaxCacheSize, cachePath, DateTime.UtcNow, cacheError);
					temporaryPath = null;
					return completedStream;
				}
				finally
				{
					reservation?.Dispose();
					if (temporaryPath is not null)
						TryDelete(new FileInfo(temporaryPath), cacheError);
				}
			}
			catch (IOException ex)
			{
				// A cache failure must not prevent a successfully downloaded image from loading.
				cacheError?.Invoke(ex);
				if (!cacheFileCreated)
					return downloaded;

				downloaded?.Dispose();
			}
			catch (UnauthorizedAccessException ex)
			{
				// A cache failure must not prevent a successfully downloaded image from loading.
				cacheError?.Invoke(ex);
				if (!cacheFileCreated)
					return downloaded;

				downloaded?.Dispose();
			}
			catch
			{
				downloaded?.Dispose();
				throw;
			}

			return await download(cancellationToken).ConfigureAwait(false);
		}

		internal static string GetCachePath(Uri uri, string cacheDirectory)
		{
			byte[] hash;
			using (var sha256 = SHA256.Create())
				hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));

			var key = string.Concat(hash.Select(value => value.ToString("x2")));
			return Path.Combine(cacheDirectory, "com.microsoft.maui", CacheFolderName, key);
		}

		static bool IsCacheValid(string cachePath, TimeSpan cacheValidity)
		{
			if (cacheValidity <= TimeSpan.Zero)
				return false;

			if (!File.Exists(cachePath))
				return false;

			if (new FileInfo(cachePath).Length == 0)
				return false;

			return cacheValidity == TimeSpan.MaxValue ||
				DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath) < cacheValidity;
		}

		static FileStream? TryOpenCachedFile(string cachePath, TimeSpan cacheValidity, Action<Exception>? cacheError)
		{
			try
			{
				if (!IsCacheValid(cachePath, cacheValidity))
				{
					if (File.Exists(cachePath))
						File.Delete(cachePath);
					return null;
				}

				File.SetLastAccessTimeUtc(cachePath, DateTime.UtcNow);
				return OpenCacheFile(cachePath);
			}
			catch (IOException ex)
			{
				cacheError?.Invoke(ex);
				return null;
			}
			catch (UnauthorizedAccessException ex)
			{
				cacheError?.Invoke(ex);
				return null;
			}
		}

		static FileStream OpenCacheFile(string cachePath) =>
			new(cachePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

		static async Task<CopyResult> CopyToCacheAsync(
			Stream source,
			Stream destination,
			long maxCacheEntrySize,
			TemporaryCacheReservation reservation,
			CancellationToken cancellationToken)
		{
			var buffer = new byte[81920];
			long totalBytes = 0;
			int bytesRead;

			while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
			{
				totalBytes += bytesRead;
				if (totalBytes > maxCacheEntrySize || !reservation.TryReserve(bytesRead))
				{
					var overflow = new byte[bytesRead];
					Array.Copy(buffer, overflow, bytesRead);
					return new CopyResult(overflow);
				}

				await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
			}

			return new CopyResult(overflow: null);
		}

		internal static void TrimCache(
			string cacheDirectory,
			long maxCacheSize,
			string? protectedPath,
			DateTime utcNow,
			Action<Exception>? cacheError = null,
			bool removeAllTemporaryFiles = false)
		{
			var directory = Path.Combine(cacheDirectory, "com.microsoft.maui", CacheFolderName);
			if (!Directory.Exists(directory))
				return;

			lock (s_trimSync)
			{
				try
				{
					var files = new DirectoryInfo(directory).GetFiles();
					foreach (var temporaryFile in files.Where(file =>
						file.Name.EndsWith(".tmp", StringComparison.Ordinal) &&
						removeAllTemporaryFiles))
					{
						TryDelete(temporaryFile, cacheError);
					}

					files = new DirectoryInfo(directory).GetFiles()
						.Where(file => !file.Name.EndsWith(".tmp", StringComparison.Ordinal))
						.OrderBy(file => file.LastAccessTimeUtc)
						.ToArray();

					var totalSize = files.Sum(file => file.Length);
					foreach (var file in files)
					{
						if (totalSize <= maxCacheSize)
							break;

						if (string.Equals(file.FullName, protectedPath, StringComparison.Ordinal))
							continue;

						var length = file.Length;
						if (TryDelete(file, cacheError))
							totalSize -= length;
					}
				}

				catch (IOException ex)
				{
					cacheError?.Invoke(ex);
				}
				catch (UnauthorizedAccessException ex)
				{
					cacheError?.Invoke(ex);
				}
			}
		}

		static void TrimCacheOnce(string cacheDirectory, Action<Exception>? cacheError)
		{
			lock (s_trimSync)
			{
				if (!s_trimmedDirectories.Add(cacheDirectory))
					return;

				TrimCache(
					cacheDirectory,
					MaxCacheSize,
					protectedPath: null,
					DateTime.UtcNow,
					cacheError,
					removeAllTemporaryFiles: true);
			}
		}

		static bool TryDelete(FileInfo file, Action<Exception>? cacheError)
		{
			try
			{
				file.Delete();
				return true;
			}
			catch (IOException ex)
			{
				cacheError?.Invoke(ex);
				return false;
			}
			catch (UnauthorizedAccessException ex)
			{
				cacheError?.Invoke(ex);
				return false;
			}
		}

		static async Task<IDisposable> AcquireLockAsync(string key, CancellationToken cancellationToken)
		{
			CacheLock cacheLock;
			lock (s_locksSync)
			{
				if (!s_locks.TryGetValue(key, out cacheLock!))
				{
					cacheLock = new CacheLock();
					s_locks.Add(key, cacheLock);
				}

				cacheLock.ReferenceCount++;
			}

			try
			{
				await cacheLock.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
				return new LockReleaser(key, cacheLock);
			}
			catch
			{
				ReleaseReference(key, cacheLock, releaseSemaphore: false);
				throw;
			}
		}

		static void ReleaseReference(string key, CacheLock cacheLock, bool releaseSemaphore)
		{
			if (releaseSemaphore)
				cacheLock.Semaphore.Release();

			lock (s_locksSync)
			{
				cacheLock.ReferenceCount--;
				if (cacheLock.ReferenceCount == 0)
					s_locks.Remove(key);
			}
		}

		sealed class CacheLock
		{
			public int ReferenceCount { get; set; }

			public SemaphoreSlim Semaphore { get; } = new(1, 1);
		}

		sealed class LockReleaser : IDisposable
		{
			readonly string _key;
			readonly CacheLock _cacheLock;
			bool _disposed;

			public LockReleaser(string key, CacheLock cacheLock)
			{
				_key = key;
				_cacheLock = cacheLock;
			}

			public void Dispose()
			{
				if (_disposed)
					return;

				_disposed = true;
				ReleaseReference(_key, _cacheLock, releaseSemaphore: true);
			}
		}

		readonly struct CopyResult
		{
			public CopyResult(byte[]? overflow) => Overflow = overflow;

			public byte[]? Overflow { get; }
		}

		sealed class TemporaryAndNetworkStream : Stream
		{
			readonly FileStream _fileStream;
			readonly MemoryStream _overflowStream;
			readonly Stream _networkStream;
			readonly IDisposable _reservation;
			readonly string _path;
			int _currentStream;

			public TemporaryAndNetworkStream(string path, byte[] overflow, Stream networkStream, IDisposable reservation)
			{
				_path = path;
				_fileStream = OpenCacheFile(path);
				_overflowStream = new MemoryStream(overflow, writable: false);
				_networkStream = networkStream;
				_reservation = reservation;
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
			public override void Flush() { }

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (count == 0)
					return 0;

				while (_currentStream < 3)
				{
					var bytesRead = GetCurrentStream().Read(buffer, offset, count);
					if (bytesRead != 0)
						return bytesRead;

					_currentStream++;
				}

				return 0;
			}

			public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				if (count == 0)
					return 0;

				while (_currentStream < 3)
				{
					var bytesRead = await GetCurrentStream().ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
					if (bytesRead != 0)
						return bytesRead;

					_currentStream++;
				}

				return 0;
			}

			Stream GetCurrentStream() =>
				_currentStream switch
				{
					0 => _fileStream,
					1 => _overflowStream,
					_ => _networkStream,
				};

			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
				_fileStream.Dispose();
				_overflowStream.Dispose();
				_networkStream.Dispose();
				_reservation.Dispose();
				}

				try
				{
					File.Delete(_path);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}

				base.Dispose(disposing);
			}
		}

		sealed class TemporaryFileStream : Stream
		{
			readonly Stream _stream;
			readonly string _path;

			public TemporaryFileStream(Stream stream, string path)
			{
				_stream = stream;
				_path = path;
			}

			public override bool CanRead => _stream.CanRead;
			public override bool CanSeek => _stream.CanSeek;
			public override bool CanWrite => false;
			public override long Length => _stream.Length;
			public override long Position { get => _stream.Position; set => _stream.Position = value; }
			public override void Flush() => _stream.Flush();
			public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
			public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
				_stream.ReadAsync(buffer, offset, count, cancellationToken);
			public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_stream.Dispose();

				TryDelete(new FileInfo(_path), cacheError: null);
				base.Dispose(disposing);
			}
		}

		sealed class TemporaryCacheReservation : IDisposable
		{
			long _bytes;

			public bool TryReserve(int bytes)
			{
				lock (s_temporaryCacheSync)
				{
					if (s_temporaryCacheSize + bytes > MaxTemporaryCacheSize)
						return false;

					s_temporaryCacheSize += bytes;
					_bytes += bytes;
					return true;
				}
			}

			public void Dispose()
			{
				lock (s_temporaryCacheSync)
				{
					s_temporaryCacheSize -= _bytes;
					_bytes = 0;
				}
			}
		}
	}
}
