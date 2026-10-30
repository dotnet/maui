using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// On-disk tile cache: <c>&lt;root&gt;/ab/cd/&lt;sha256&gt;.tile</c>, written atomically. Entries older than <see cref="MaxAge"/> count as
	/// missing (the OpenStreetMap tile policy asks clients to cache for at least a week and to honour server expiry).
	/// </summary>
	internal sealed class TileDiskCache
	{
		public TileDiskCache(string root, TimeSpan maxAge)
		{
			Root = root;
			MaxAge = maxAge;
		}

		public string Root { get; }

		public TimeSpan MaxAge { get; }

		public string PathFor(string url)
		{
			var digest = SHA256.HashData(Encoding.UTF8.GetBytes(url));
			var hash = Convert.ToHexString(digest).ToLowerInvariant();
			return Path.Combine(Root, hash.Substring(0, 2), hash.Substring(2, 2), hash + ".tile");
		}

		/// <summary>Reads a fresh entry, or returns <see langword="null"/>.</summary>
		public byte[]? TryRead(string url)
		{
			var path = PathFor(url);
			try
			{
				var info = new FileInfo(path);
				if (!info.Exists || info.Length == 0)
					return null;

				if (MaxAge > TimeSpan.Zero && DateTime.UtcNow - info.LastWriteTimeUtc > MaxAge)
					return null;

				return File.ReadAllBytes(path);
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException)
			{
				return null;
			}
		}

		/// <summary>Writes an entry; failures are ignored (a cold cache is not an error).</summary>
		public void TryWrite(string url, byte[] bytes)
		{
			var path = PathFor(url);
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
				File.WriteAllBytes(temporary, bytes);
				File.Move(temporary, path, overwrite: true);
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException)
			{
			}
		}

		/// <summary>Deletes the least recently written entries until the cache is at most <paramref name="maxBytes"/>.</summary>
		/// <returns>The number of bytes deleted.</returns>
		public long Trim(long maxBytes)
		{
			if (maxBytes <= 0 || !Directory.Exists(Root))
				return 0;

			var files = new List<FileInfo>();
			long total = 0;

			try
			{
				foreach (var file in new DirectoryInfo(Root).EnumerateFiles("*.tile", SearchOption.AllDirectories))
				{
					files.Add(file);
					total += file.Length;
				}
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException)
			{
				return 0;
			}

			if (total <= maxBytes)
				return 0;

			files.Sort(static (a, b) => a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc));

			long deleted = 0;
			foreach (var file in files)
			{
				if (total - deleted <= maxBytes)
					break;

				try
				{
					long length = file.Length;
					file.Delete();
					deleted += length;
				}
				catch (Exception e) when (e is IOException or UnauthorizedAccessException)
				{
				}
			}

			return deleted;
		}
	}
}
