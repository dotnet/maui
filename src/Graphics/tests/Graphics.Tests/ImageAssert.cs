using System.IO;
using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace Microsoft.Maui;

public static class ImageAssert
{
	private const double ImageErrorThreshold = 0.0027;

	public static void Equivalent(Stream actualData, string expectedFilename, string diffDirectory, double threshold = ImageErrorThreshold)
	{
		using var actual = SKImage.FromEncodedData(actualData);
		Equivalent(actual, expectedFilename, diffDirectory, threshold);
	}

	public static void Equivalent(string actualFilename, string expectedFilename, string diffDirectory, double threshold = ImageErrorThreshold)
	{
		Assert.True(File.Exists(actualFilename), $"Actual file does not exist: {actualFilename}");

		using var actual = SKImage.FromEncodedData(actualFilename);
		Equivalent(actual, expectedFilename, diffDirectory, threshold);
	}

	public static void Equivalent(SKImage actual, string expectedFilename, string diffDirectory, double threshold = ImageErrorThreshold)
	{
		Assert.True(File.Exists(expectedFilename), $"Expected file does not exist: {expectedFilename}");

		var expected = SKImage.FromEncodedData(expectedFilename);
		var similarity = SKPixelComparer.Compare(actual, expected);

		var isSimilar = similarity.ErrorPixelPercentage <= threshold;

		if (isSimilar)
		{
			return;
		}

		var outputFilename = Path.Combine(diffDirectory, expectedFilename);

		var diffFilename = Path.ChangeExtension(outputFilename, ".diff.png");

		Directory.CreateDirectory(Path.GetDirectoryName(diffFilename));

		using (var diff = SKPixelComparer.GenerateDifferenceMask(actual, expected))
		using (var diffData = diff.Encode(SKEncodedImageFormat.Png, 100))
		using (var diffFile = File.Create(diffFilename))
		{
			diffData.SaveTo(diffFile);
		}

		using (var actualData = actual.Encode(SKEncodedImageFormat.Png, 100))
		using (var actualFile = File.Create(Path.ChangeExtension(outputFilename, ".png")))
		{
			actualData.SaveTo(actualFile);
		}

		File.Copy(expectedFilename, Path.ChangeExtension(outputFilename, ".expected.png"), true);

		Assert.Fail($"Image was not equal. Error was {similarity.ErrorPixelPercentage}% ({similarity.AbsoluteError} pixels), which was higher than the theshold of {ImageErrorThreshold}%. See {diffFilename}");
	}
}
