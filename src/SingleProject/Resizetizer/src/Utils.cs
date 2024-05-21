using System;
using System.IO;
using System.Text.RegularExpressions;
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
			var exists = File.Exists(path);
			var modified = exists ? File.GetLastWriteTimeUtc(path) : System.DateTime.MinValue;
			return (exists, modified);
		}
	}
}
