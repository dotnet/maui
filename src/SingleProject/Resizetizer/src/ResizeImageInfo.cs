#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class ResizeImageInfo
	{
		public string? ItemSpec { get; set; }

		public string? Alias { get; set; }

		public string? Filename { get; set; }

		public string OutputName =>
			string.IsNullOrWhiteSpace(Alias)
				? string.IsNullOrWhiteSpace(Filename)
					? Path.GetFileNameWithoutExtension(ForegroundFilename)
					: Path.GetFileNameWithoutExtension(Filename)
				: Path.GetFileNameWithoutExtension(Alias);

		public string OutputExtension =>
			string.IsNullOrWhiteSpace(Alias) || !Path.HasExtension(Alias)
				? string.IsNullOrWhiteSpace(Filename) || !Path.HasExtension(Filename)
					? Path.GetExtension(ForegroundFilename)
					: Path.GetExtension(Filename)
				: Path.GetExtension(Alias);

		public bool OutputIsVector => IsVectorExtension(OutputExtension);

		public SKSize? BaseSize { get; set; }

		public bool Resize { get; set; } = true;

		public SKColor? TintColor { get; set; }

		public SKColor? Color { get; set; }

		public SKColor? DarkTintColor { get; set; }

		public SKColor? DarkColor { get; set; }

		public bool IsVector => IsVectorFilename(Filename);

		public string? DarkFilename { get; set; }

		public bool DarkIsVector => IsVectorFilename(DarkFilename);

		public bool HasDarkMode =>
			DarkColor is not null ||
			DarkTintColor is not null ||
			!string.IsNullOrWhiteSpace(DarkFilename);

		public bool IsAppIcon { get; set; }

		public string? ForegroundFilename { get; set; }

		public bool ForegroundIsVector => IsVectorFilename(ForegroundFilename);

		public double ForegroundScale { get; set; } = 1.0;

		private static bool IsVectorFilename(string? filename)
			=> IsVectorExtension(Path.GetExtension(filename));

		private static bool IsVectorExtension(string? extension)
			=> extension?.Equals(".svg", StringComparison.OrdinalIgnoreCase) ?? false;

		public static List<ResizeImageInfo> Parse(IEnumerable<ITaskItem> images)
		{
			var r = new List<ResizeImageInfo>();

			if (images == null)
				return r;

			foreach (var image in images)
			{
				var info = Parse(image);
				r.Add(info);
			}

			return r;
		}

		public static ResizeImageInfo Parse(ITaskItem image)
		{
			var info = new ResizeImageInfo();

			info.ItemSpec = image.ItemSpec;

			var fileInfo = new FileInfo(image.GetMetadata("FullPath"));
			if (!fileInfo.Exists)
				throw new FileNotFoundException("Unable to find background file: " + fileInfo.FullName, fileInfo.FullName);

			info.Filename = fileInfo.FullName;

			info.Alias = image.GetMetadata("Link");

			info.BaseSize = Utils.ParseSizeString(image.GetMetadata("BaseSize"));

			if (bool.TryParse(image.GetMetadata("Resize"), out var rz))
			{
				info.Resize = rz;
			}
			else if (info.BaseSize == null && !info.IsVector)
			{
				// By default do not resize non-vector images
				info.Resize = false;
			}

			var tintColor = image.GetMetadata("TintColor");
			info.TintColor = Utils.ParseColorString(tintColor);
			if (info.TintColor is null && !string.IsNullOrEmpty(tintColor))
				throw new InvalidDataException($"Unable to parse color value '{tintColor}' for '{info.Filename}'.");

			var color = image.GetMetadata("Color");
			info.Color = Utils.ParseColorString(color);
			if (info.Color is null && !string.IsNullOrEmpty(color))
				throw new InvalidDataException($"Unable to parse color value '{color}' for '{info.Filename}'.");

			var darkTintColor = image.GetMetadata("DarkTintColor");
			info.DarkTintColor = Utils.ParseColorString(darkTintColor);
			if (info.DarkTintColor is null && !string.IsNullOrEmpty(darkTintColor))
				throw new InvalidDataException($"Unable to parse color value '{darkTintColor}' for '{info.Filename}'.");

			var darkColor = image.GetMetadata("DarkColor");
			info.DarkColor = Utils.ParseColorString(darkColor);
			if (info.DarkColor is null && !string.IsNullOrEmpty(darkColor))
				throw new InvalidDataException($"Unable to parse color value '{darkColor}' for '{info.Filename}'.");

			if (bool.TryParse(image.GetMetadata("IsAppIcon"), out var iai))
				info.IsAppIcon = iai;

			if (float.TryParse(image.GetMetadata("ForegroundScale"), NumberStyles.Number, CultureInfo.InvariantCulture, out var fsc))
				info.ForegroundScale = fsc;

			var fgFile = image.GetMetadata("ForegroundFile");
			if (!string.IsNullOrEmpty(fgFile))
			{
				var fgFileInfo = new FileInfo(fgFile);
				if (!fgFileInfo.Exists)
					throw new FileNotFoundException("Unable to find foreground file: " + fgFileInfo.FullName, fgFileInfo.FullName);

				info.ForegroundFilename = fgFileInfo.FullName;
			}

			var darkFile = image.GetMetadata("DarkFile");
			if (!string.IsNullOrEmpty(darkFile))
			{
				var darkFileInfo = new FileInfo(darkFile);
				if (!darkFileInfo.Exists)
					throw new FileNotFoundException("Unable to find dark file: " + darkFileInfo.FullName, darkFileInfo.FullName);

				info.DarkFilename = darkFileInfo.FullName;
			}

			// make sure the image is a foreground if this is an icon
			if (info.IsAppIcon && string.IsNullOrEmpty(info.ForegroundFilename))
			{
				info.ForegroundFilename = info.Filename;
				info.Filename = null;
			}

			// TODO:
			// - Parse out custom DPI's

			return info;
		}

		public ResizeImageInfo CreateDarkVariant(string? alias = null)
		{
			var hasDarkFile = !string.IsNullOrWhiteSpace(DarkFilename);

			return new ResizeImageInfo
			{
				ItemSpec = ItemSpec,
				Alias = alias ?? Alias,
				Filename = hasDarkFile ? DarkFilename : Filename,
				BaseSize = BaseSize,
				Resize = Resize,
				TintColor = DarkTintColor ?? (hasDarkFile ? null : TintColor),
				Color = DarkColor ?? Color,
				IsAppIcon = IsAppIcon,
				ForegroundFilename = ForegroundFilename,
				ForegroundScale = ForegroundScale,
			};
		}
	}
}
