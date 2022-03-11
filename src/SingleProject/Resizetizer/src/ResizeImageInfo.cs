using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class ResizeImageInfo
	{
		public string Alias { get; set; }

		public string Filename { get; set; }

		public string OutputName =>
			string.IsNullOrWhiteSpace(Alias)
				? Path.GetFileNameWithoutExtension(Filename)
				: Path.GetFileNameWithoutExtension(Alias);

		public string OutputExtension =>
			string.IsNullOrWhiteSpace(Alias) || !Path.HasExtension(Alias)
				? Path.GetExtension(Filename)
				: Path.GetExtension(Alias);

		public SKSize? BaseSize { get; set; }

		public bool Resize { get; set; } = true;

		public SKColor? TintColor { get; set; }

		public SKColor? Color { get; set; }

		public bool IsVector => IsVectorFilename(Filename);

		public bool IsAppIcon { get; set; }

		public string ForegroundFilename { get; set; }

		public bool ForegroundIsVector => IsVectorFilename(ForegroundFilename);

		public double ForegroundScale { get; set; } = 1.0;

		private static bool IsVectorFilename(string filename)
			=> Path.GetExtension(filename)?.Equals(".svg", StringComparison.OrdinalIgnoreCase) ?? false;

		public static ResizeImageInfo Parse(ITaskItem image)
			=> Parse(new[] { image })[0];

		public static List<ResizeImageInfo> Parse(IEnumerable<ITaskItem> images)
		{
			var r = new List<ResizeImageInfo>();

			if (images == null)
				return r;

			foreach (var image in images)
			{
				var info = new ResizeImageInfo();

				var fileInfo = new FileInfo(image.GetMetadata("FullPath"));
				if (!fileInfo.Exists)
					throw new FileNotFoundException("Unable to find background file: " + fileInfo.FullName, fileInfo.FullName);

				info.Filename = fileInfo.FullName;

				info.Alias = image.GetMetadata("Link");

				info.BaseSize = Utils.ParseSizeString(image.GetMetadata("BaseSize"));

				if (bool.TryParse(image.GetMetadata("Resize"), out var rz))
					info.Resize = rz;

				info.TintColor = Utils.ParseColorString(image.GetMetadata("TintColor"));
				info.Color = Utils.ParseColorString(image.GetMetadata("Color"));

				if (bool.TryParse(image.GetMetadata("IsAppIcon"), out var iai))
					info.IsAppIcon = iai;

				if (float.TryParse(image.GetMetadata("ForegroundScale"), out var fsc))
					info.ForegroundScale = fsc;

				var fgFile = image.GetMetadata("ForegroundFile");
				if (!string.IsNullOrEmpty(fgFile))
				{
					var fgFileInfo = new FileInfo(fgFile);
					if (!fgFileInfo.Exists)
						throw new FileNotFoundException("Unable to find foreground file: " + fgFileInfo.FullName, fgFileInfo.FullName);

					info.ForegroundFilename = fgFileInfo.FullName;
				}

				// TODO:
				// - Parse out custom DPI's

				r.Add(info);
			}

			return r;
		}
	}
}
