using System.Collections.Generic;
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
			tools ??= SkiaSharpTools.Create(Info.IsVector, Info.Filename, Info.BaseSize, Info.Color, Info.TintColor, Info.Quality, Logger);

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

		internal static bool IsUpToDate(string inputFile, string outputFile, string inputsFile, ILogger logger)
		{
			return IsUpToDate(new[] { inputFile }, outputFile, inputsFile, logger, inputFile);
		}

		internal static bool IsUpToDate(IEnumerable<string> inputFiles, string outputFile, string inputsFile, ILogger logger, string inputDescription = null)
		{
			var fileInputs = string.IsNullOrEmpty(inputsFile) ? null : new FileInfo(inputsFile);
			if (fileInputs?.Exists != true)
				return false;

			var fileOut = new FileInfo(outputFile);
			if (!fileOut.Exists)
				return false;

			var newestInput = fileInputs.LastWriteTimeUtc;

			foreach (var inputFile in inputFiles)
			{
				if (string.IsNullOrEmpty(inputFile))
					continue;

				var fileIn = new FileInfo(inputFile);
				if (!fileIn.Exists)
					return false;

				if (fileIn.LastWriteTimeUtc > newestInput)
					newestInput = fileIn.LastWriteTimeUtc;
			}

			if (newestInput > fileOut.LastWriteTimeUtc)
				return false;

			var description = string.IsNullOrEmpty(inputDescription) ? "inputs" : inputDescription;
			logger.Log($"Skipping '{description}' as output '{outputFile}' is already up to date.");
			return true;
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
