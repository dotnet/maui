using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Build.Framework;
using SkiaSharp;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Maui.Resizetizer.Tests")]

namespace Microsoft.Maui.Resizetizer
{
	internal class Utils
	{
		static readonly Regex rxResourceFilenameValidation
			= new Regex(@"^[a-z]([a-z0-9_]*[a-z0-9])?$", RegexOptions.Singleline | RegexOptions.Compiled);

		public static bool IsValidResourceFilename(string filename)
			=> rxResourceFilenameValidation.IsMatch(Path.GetFileNameWithoutExtension(filename));

		public static SKColor? ParseColorString(string tint)
		{
			if (string.IsNullOrEmpty(tint))
				return null;

			if (SKColor.TryParse(tint, out var color))
			{
				return color;
			}

			if (ColorTable.TryGetNamedColor(tint, out color))
			{
				return color;
			}

			return null;
		}

		public static SKSize? ParseSizeString(string size)
		{
			if (string.IsNullOrEmpty(size))
				return null;

			var parts = size.Split(new char[] { ',', ';' }, 2);

			if (parts.Length > 0 && int.TryParse(parts[0], out var width))
			{
				if (parts.Length > 1 && int.TryParse(parts[1], out var height))
					return new SKSize(width, height);
				else
					return new SKSize(width, width);
			}

			return null;
		}

		public static (bool Exists, DateTime Modified) FileExists(string path)
		{
			if (string.IsNullOrEmpty(path))
				return (false, DateTime.MinValue);

			AbsolutePath file = new(path);
			var exists = File.Exists(file);
			var modified = exists ? File.GetLastWriteTimeUtc(file) : DateTime.MinValue;
			return (exists, modified);
		}

		public static void SetWriteable(string source, bool checkExists = true)
		{
			AbsolutePath file = new(source);
			if (checkExists && !File.Exists(file))
				return;

			var attributes = File.GetAttributes(file);
			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
		}

		/// <summary>
		/// Deletes a file, retrying while another process briefly holds it open.
		/// </summary>
		public static void DeleteFileWithRetries(string path, FileRetryPolicy retryPolicy)
		{
			AbsolutePath file = new(path);
			RetryWhileFileIsLocked(() => File.Delete(file), retryPolicy.Attempts, retryPolicy.DelayMilliseconds);
		}

		/// <summary>
		/// Recursively deletes a directory, retrying while another process briefly holds one
		/// of its files open.
		/// </summary>
		public static void DeleteDirectoryWithRetries(string path, FileRetryPolicy retryPolicy)
		{
			AbsolutePath directory = new(path);
			RetryWhileFileIsLocked(() => Directory.Delete(directory, recursive: true), retryPolicy.Attempts, retryPolicy.DelayMilliseconds);
		}

		internal static void RetryWhileFileIsLocked(Action action, int attempts, int delay)
		{
			for (var attempt = 0; ; attempt++)
			{
				try
				{
					action();
					return;
				}
				catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) && attempt < attempts && IsFileLocked(ex))
				{
					Thread.Sleep(delay);
				}
			}
		}

		static bool IsFileLocked(Exception ex)
		{
			var code = Marshal.GetHRForException(ex);
			return code == SkiaSharpTools.ERROR_ACCESS_DENIED || code == SkiaSharpTools.ERROR_SHARING_VIOLATION;
		}
	}
}
