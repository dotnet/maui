using System;
using System.IO;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Generates a .ico file for the image.
	/// </summary>
	internal class WindowsIconGenerator
	{
		public WindowsIconGenerator(ResizeImageInfo info, string intermediateOutputPath, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public ResizeImageInfo Info { get; private set; }
		public string IntermediateOutputPath { get; private set; }
		public ILogger Logger { get; private set; }

		public ResizedImageInfo Generate()
		{
			string destinationFolder = IntermediateOutputPath;

			string fileName = Path.GetFileNameWithoutExtension(Info.OutputName);
			string destination = Path.Combine(destinationFolder, $"{fileName}.ico");
			Directory.CreateDirectory(destinationFolder);

			var (sourceExists, sourceModified) = Utils.FileExists(Info.Filename);
			var (destinationExists, destinationModified) = Utils.FileExists(destination);

			Logger.Log($"Generating ICO: {destination}");

			var tools = new SkiaSharpAppIconTools(Info, Logger);
			var dpi = new DpiPath(fileName, 1.0m, size: new SKSize(64, 64));

			if (destinationModified > sourceModified)
			{
				Logger.Log($"Skipping `{Info.Filename}` => `{destination}` file is up to date.");
				return new ResizedImageInfo { Dpi = dpi, Filename = destination };
			}

			using MemoryStream memoryStream = new MemoryStream();
			tools.Resize(dpi, destination, memoryStream);
			memoryStream.Position = 0;

			int numberOfImages = 1;
			using BinaryWriter writer = new BinaryWriter(File.Create(destination));
			writer.Write((short)0x0); // Reserved. Must always be 0.
			writer.Write((short)0x1); // Specifies image type: 1 for icon (.ICO) image
			writer.Write((short)numberOfImages); // Specifies number of images in the file.

			writer.Write((byte)dpi.Size.Value.Width);
			writer.Write((byte)dpi.Size.Value.Height);
			writer.Write((byte)0x0); // Specifies number of colors in the color palette
			writer.Write((byte)0x0); // Reserved. Should be 0
			writer.Write((short)0x1); // Specifies color planes. Should be 0 or 1
			writer.Write((short)0x8); // Specifies bits per pixel.
			writer.Write((int)memoryStream.Length); // Specifies the size of the image's data in bytes

			int offset = 6 + (16 * numberOfImages); // + length of previous images
			writer.Write(offset); // Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file

			// write png data for each image
			memoryStream.CopyTo(writer.BaseStream);
			writer.Flush();

			return new ResizedImageInfo { Dpi = dpi, Filename = destination };
		}
	}
}
