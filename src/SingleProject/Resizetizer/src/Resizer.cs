using System.IO;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class Resizer
	{
		public const string RasterFileExtension = ".png";

		SkiaSharpTools tools;

		public Resizer(ResizeImageInfo info, string intermediateOutputPath, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public ILogger Logger { get; private set; }

		public string IntermediateOutputPath { get; private set; }

		public ResizeImageInfo Info { get; private set; }

		public SKSize? BaseSize => Info.BaseSize;

		protected SkiaSharpTools Tools =>
			tools ??= SkiaSharpTools.Create(Info.IsVector, Info.Filename, Info.BaseSize, Info.Color, Info.TintColor, Logger);

		public string GetRasterFileDestination(DpiPath dpi, bool includeIntermediate = true, bool includeScale = true)
			=> GetRasterFileDestination(Info, dpi, includeIntermediate ? IntermediateOutputPath : null, includeScale);

		public string GetFileDestination(DpiPath dpi, bool includeIntermediate = true, bool includeScale = true)
			=> GetFileDestination(Info, dpi, includeIntermediate ? IntermediateOutputPath : null, includeScale);

		public static string GetRasterFileDestination(ResizeImageInfo info, DpiPath dpi, string intermediateOutputPath = default, bool includeScale = true)
		{
			var destination = GetFileDestination(info, dpi, intermediateOutputPath, includeScale);

			if (info.OutputIsVector)
				destination = Path.ChangeExtension(destination, RasterFileExtension);

			return destination;
		}

		public static string GetFileDestination(ResizeImageInfo info, DpiPath dpi, string intermediateOutputPath = default, bool includeScale = true)
		{
			var destination = Path.Combine(dpi.Path, info.OutputName + (includeScale ? dpi.FileSuffix : dpi.NameSuffix) + info.OutputExtension);

			if (!string.IsNullOrEmpty(intermediateOutputPath))
			{
				var fullIntermediateOutputPath = new DirectoryInfo(intermediateOutputPath);
				destination = Path.Combine(fullIntermediateOutputPath.FullName, destination);
			}

			var fileInfo = new FileInfo(destination);
			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			return destination;
		}

		public ResizedImageInfo CopyFile(DpiPath dpi, string inputsFile)
		{
			var destination = GetRasterFileDestination(dpi);

			if (IsUpToDate(Info.Filename, destination, inputsFile, Logger))
				return new ResizedImageInfo { Filename = destination, Dpi = dpi };

			if (Info.IsVector)
				Rasterize(dpi, destination);
			else
				File.Copy(Info.Filename, destination, true);

			return new ResizedImageInfo { Filename = destination, Dpi = dpi };
		}

		static bool IsUpToDate(string inputFile, string outputFile, string inputsFile, ILogger logger)
		{
			var fileIn = new FileInfo(inputFile);
			var fileOut = new FileInfo(outputFile);
			var fileInputs = inputsFile is null ? null : new FileInfo(inputsFile);

			if (fileIn.Exists && fileOut.Exists && fileInputs?.Exists == true
				&& fileIn.LastWriteTimeUtc <= fileOut.LastWriteTimeUtc
				&& fileInputs.LastWriteTimeUtc <= fileOut.LastWriteTimeUtc)
			{
				logger.Log($"Skipping '{inputFile}' as output '{outputFile}' is already up to date.");
				return true;
			}

			return false;
		}

		public ResizedImageInfo Resize(DpiPath dpi, string inputsFile)
		{
			var destination = GetFileDestination(dpi);

			if (Info.IsVector)
				destination = Path.ChangeExtension(destination, RasterFileExtension);

			if (IsUpToDate(Info.Filename, destination, inputsFile, Logger))
				return new ResizedImageInfo { Filename = destination, Dpi = dpi };

			Rasterize(dpi, destination);

			return new ResizedImageInfo { Filename = destination, Dpi = dpi };
		}

		public SKSize GetOriginalSize() =>
			Tools.GetOriginalSize();

		void Rasterize(DpiPath dpi, string destination) =>
			Tools.Resize(dpi, destination);
	}
}
