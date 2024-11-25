using System;
using System.IO;
using System.Text.RegularExpressions;
using SkiaSharp;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Maui.Resizetizer.Tests")]

namespace Microsoft.Maui.Resizetizer
{
	internal class Utils
	{
		public static bool IsValidResourceFilename(string filename)
			=> RegexHelper.ResourceFilenameRegex.IsMatch(Path.GetFileNameWithoutExtension(filename));

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
			var exists = File.Exists(path);
			var modified = exists ? File.GetLastWriteTimeUtc(path) : System.DateTime.MinValue;
			return (exists, modified);
		}

		public static void SetWriteable(string source, bool checkExists = true)
		{
			if (checkExists && !File.Exists(source))
				return;

			var attributes = File.GetAttributes(source);
			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				File.SetAttributes(source, attributes & ~FileAttributes.ReadOnly);
		}

		static readonly string ResourceFilenameRegexPattern = @"^[a-z]([a-z0-9_]*[a-z0-9])?$";

		internal static partial class RegexHelper
		{
#if NET7_0_OR_GREATER
			[GeneratedRegex (ResourceFilenameRegexPattern, RegexOptions.Singleline, matchTimeoutMilliseconds: 1000))]
			static partial Regex ResourceFilenameRegex
			{
				get;
			}
#else
			public static readonly Regex ResourceFilenameRegex =
											new (
												ResourceFilenameRegexPattern,
												RegexOptions.Compiled | RegexOptions.Singleline,		
												TimeSpan.FromMilliseconds(1000)							// against malicious input
												);
#endif
		}
	}
}
