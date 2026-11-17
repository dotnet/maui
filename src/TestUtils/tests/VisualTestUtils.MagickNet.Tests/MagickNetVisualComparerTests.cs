using System;
using System.Globalization;
using ImageMagick;
using Xunit;

namespace VisualTestUtils.MagickNet.Tests;

public class MagickNetVisualComparerTests
{
	[Theory]
	[InlineData(0, 1)]
	[InlineData(4, 2)]
	[InlineData(2, 3)]
	[InlineData(6, 4)]
	public void RedChannelRmsePreservesLegacyNormalization(int pngColorType, int channelCount)
	{
		var baseline = CreateSnapshot(64, 64, 64, pngColorType);
		var actual = CreateSnapshot(66, 66, 66, pngColorType);
		double expectedDifference = 2.0 / 255 / Math.Sqrt(channelCount);
		var acceptingComparer = new MagickNetVisualComparer(differenceThreshold: expectedDifference + 0.000001);
		var rejectingComparer = new MagickNetVisualComparer(differenceThreshold: expectedDifference - 0.000001);

		Assert.Null(acceptingComparer.Compare(baseline, actual));
		var difference = Assert.IsType<ImagePercentageDifference>(rejectingComparer.Compare(baseline, actual));
		Assert.Equal(new ImagePercentageDifference(expectedDifference).Description, difference.Description);
	}

	[Theory]
	[InlineData(2, 6, 3)]
	[InlineData(6, 2, 4)]
	public void NormalizationUsesBaselineChannels(int baselineColorType, int actualColorType, int channelCount)
	{
		var baseline = CreateSnapshot(64, 96, 128, baselineColorType);
		var actual = CreateSnapshot(66, 96, 128, actualColorType);
		double expectedDifference = 2.0 / 255 / Math.Sqrt(channelCount);
		var acceptingComparer = new MagickNetVisualComparer(differenceThreshold: expectedDifference + 0.000001);
		var rejectingComparer = new MagickNetVisualComparer(differenceThreshold: expectedDifference - 0.000001);

		Assert.Null(acceptingComparer.Compare(baseline, actual));
		Assert.IsType<ImagePercentageDifference>(rejectingComparer.Compare(baseline, actual));
	}

	[Theory]
	[InlineData(2, 2, true)]
	[InlineData(2, 3, false)]
	[InlineData(6, 2, true)]
	[InlineData(6, 3, false)]
	public void DefaultThresholdPreservesPassFailBoundary(int pngColorType, byte redDifference, bool shouldMatch)
	{
		var baseline = CreateSnapshot(64, 96, 128, pngColorType);
		var actual = CreateSnapshot((byte)(64 + redDifference), 96, 128, pngColorType);

		var difference = new MagickNetVisualComparer().Compare(baseline, actual);

		if (shouldMatch)
			Assert.Null(difference);
		else
			Assert.IsType<ImagePercentageDifference>(difference);
	}

	[Fact]
	public void OtherColorChannelsRemainIgnored()
	{
		var baseline = CreateSnapshot(64, 96, 128, 6);
		var actual = CreateSnapshot(64, 255, 0, 6);

		Assert.Null(new MagickNetVisualComparer().Compare(baseline, actual));
	}

	[Fact]
	public void DifferentImageSizesRemainRejected()
	{
		var baseline = CreateSnapshot(64, 96, 128, 6);
		using var image = new MagickImage(baseline.Data);
		image.Crop(new MagickGeometry(1, 1));
		var actual = new ImageSnapshot(image.ToByteArray(MagickFormat.Png), ImageSnapshotFormat.PNG);

		Assert.IsType<ImageSizeDifference>(new MagickNetVisualComparer().Compare(baseline, actual));
	}

	[Fact]
	public void ExplicitNonRmseMetricIsNotRescaled()
	{
		var baseline = CreateSnapshot(64, 96, 128, 6);
		var actual = CreateSnapshot(66, 96, 128, 6);
		using var baselineImage = new MagickImage(baseline.Data);
		using var actualImage = new MagickImage(actual.Data);
		double expectedDifference = baselineImage.Compare(actualImage, ErrorMetric.PeakAbsolute, Channels.Red);
		var comparer = new MagickNetVisualComparer(ErrorMetric.PeakAbsolute, expectedDifference - 0.000001);

		var difference = Assert.IsType<ImagePercentageDifference>(comparer.Compare(baseline, actual));

		Assert.Equal(new ImagePercentageDifference(expectedDifference).Description, difference.Description);
	}

	static ImageSnapshot CreateSnapshot(byte red, byte green, byte blue, int pngColorType)
	{
		using var image = new MagickImage(new MagickColor(red, green, blue), 2, 2);
		image.Settings.SetDefine(MagickFormat.Png, "color-type", pngColorType.ToString(CultureInfo.InvariantCulture));
		return new ImageSnapshot(image.ToByteArray(MagickFormat.Png), ImageSnapshotFormat.PNG);
	}
}
