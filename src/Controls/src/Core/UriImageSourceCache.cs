#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Controls
{
	internal static class UriImageSourceCache
	{
		const string CacheFolderName = "MauiUriImages";

		static readonly object s_locksSync = new();
		static readonly Dictionary<string, CacheLock> s_locks = new();

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
			Action<Exception>? cacheError = null)
		{
			var cachePath = GetCachePath(uri, cacheDirectory);

			using var cacheLock = await AcquireLockAsync(cachePath, cancellationToken).ConfigureAwait(false);

			var cached = TryOpenCachedFile(cachePath, cacheValidity, cacheError);
			if (cached is not null)
				return cached;

			using var downloaded = await download(cancellationToken).ConfigureAwait(false);
			if (downloaded is null || !downloaded.CanRead)
				return null;

			var buffer = new MemoryStream();
			await downloaded.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
			buffer.Position = 0;

			try
			{
				var directory = Path.GetDirectoryName(cachePath)!;
				Directory.CreateDirectory(directory);

				var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(cachePath)}.{Guid.NewGuid():N}.tmp");
				try
				{
					using (var cacheStream = File.Create(temporaryPath))
						await buffer.CopyToAsync(cacheStream, 81920, cancellationToken).ConfigureAwait(false);

					if (File.Exists(cachePath))
						File.Replace(temporaryPath, cachePath, null);
					else
						File.Move(temporaryPath, cachePath);
				}
				finally
				{
					File.Delete(temporaryPath);
				}
			}
			catch (IOException ex)
			{
				// A cache failure must not prevent a successfully downloaded image from loading.
				cacheError?.Invoke(ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				// A cache failure must not prevent a successfully downloaded image from loading.
				cacheError?.Invoke(ex);
			}

			buffer.Position = 0;
			return buffer;
		}

		internal static string GetCachePath(Uri uri, string cacheDirectory)
		{
			var key = Crc64.ComputeHashString(uri.AbsoluteUri);
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

		static Stream? TryOpenCachedFile(string cachePath, TimeSpan cacheValidity, Action<Exception>? cacheError)
		{
			try
			{
				return IsCacheValid(cachePath, cacheValidity)
					? File.OpenRead(cachePath)
					: null;
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
	}
}
