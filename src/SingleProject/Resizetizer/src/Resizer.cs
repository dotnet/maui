using System.IO;

namespace Microsoft.Maui.Resizetizer
{
	internal class Resizer
	{
		public Resizer(ResizeImageInfo info, string intermediateOutputPath, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public ILogger Logger { get; private set; }

		public string IntermediateOutputPath { get; private set; }

		public ResizeImageInfo Info { get; private set; }

		SkiaSharpTools tools;

		public string GetFileDestination(DpiPath dpi)
			=> GetFileDestination(Info, dpi, IntermediateOutputPath);

		public static string GetFileDestination(ResizeImageInfo info, DpiPath dpi, string intermediateOutputPath)
		{
			var fullIntermediateOutputPath = new DirectoryInfo(intermediateOutputPath);

			var destination = Path.Combine(fullIntermediateOutputPath.FullName, dpi.Path, info.OutputName + dpi.FileSuffix + info.OutputExtension);

			var fileInfo = new FileInfo(destination);
			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			return destination;
		}

		public ResizedImageInfo CopyFile(DpiPath dpi, string inputsFile, bool isAndroid = false)
		{
			var destination = GetFileDestination(dpi);
			var androidVector = false;

			if (isAndroid && Info.IsVector && !Info.Resize)
			{
				// Update destination to be .xml file
				destination = Path.ChangeExtension(destination, ".xml");
				androidVector = true;
			}

			if (IsUpToDate(Info.Filename, destination, inputsFile, Logger))
				return new ResizedImageInfo { Filename = destination, Dpi = dpi };

			if (androidVector)
			{
				Logger.Log("Converting SVG to Android Drawable Vector: " + Info.Filename);

				// Transform into an android vector drawable
				Svg2VectorDrawable.Svg2Vector.Convert(Info.Filename, destination);
			}
			else
			{
				// Otherwise just copy it straight
				File.Copy(Info.Filename, destination, true);
			}

			return new ResizedImageInfo { Filename = destination, Dpi = dpi };
		}

		static bool IsUpToDate(string inputFile, string outputFile, string inputsFile, ILogger logger)
		{
			var fileIn = new FileInfo(inputFile);
			var fileOut = new FileInfo(outputFile);
			var fileInputs = new FileInfo(inputsFile);

			if (fileIn.Exists && fileOut.Exists && fileInputs.Exists
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
				destination = Path.ChangeExtension(destination, ".png");

			if (IsUpToDate(Info.Filename, destination, inputsFile, Logger))
				return new ResizedImageInfo { Filename = destination, Dpi = dpi };

			if (tools == null)
			{
				tools = SkiaSharpTools.Create(Info.IsVector, Info.Filename, Info.BaseSize, Info.TintColor, Logger);
			}

			tools.Resize(dpi, destination);

			return new ResizedImageInfo { Filename = destination, Dpi = dpi };
		}
	}
}
