using System;
using System.IO;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class ResizerTests : IDisposable
	{
		readonly string DestinationDirectory;

		public ResizerTests()
		{
			DestinationDirectory = Path.Combine(Path.GetTempPath(), nameof(ResizerTests), Path.GetRandomFileName());
			Directory.CreateDirectory(DestinationDirectory);
		}

		public void Dispose()
		{
			if (Directory.Exists(DestinationDirectory))
				Directory.Delete(DestinationDirectory, true);
		}

		[Fact]
		public void MultiInputIsNotUpToDateWhenInputsFileIsMissing()
		{
			var inputFile = Path.Combine(DestinationDirectory, "image.png");
			var outputFile = Path.Combine(DestinationDirectory, "image.out");
			var inputsFile = Path.Combine(DestinationDirectory, "mauiimage.inputs");

			File.WriteAllText(inputFile, "input");
			File.WriteAllText(outputFile, "output");
			File.SetLastWriteTimeUtc(outputFile, File.GetLastWriteTimeUtc(inputFile).AddSeconds(1));

			var logger = new TestLogger();

			Assert.False(Resizer.IsUpToDate(new[] { inputFile }, outputFile, inputsFile, logger));
			Assert.Empty(logger.Messages);
		}
	}
}
