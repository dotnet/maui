using System;
using Microsoft.Maui.Graphics.Platform;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests.Tests;

public class PlatformStringSizeServiceTests
{
	[Fact]
	public void GetStringSize_FontWeightOutOfRange_ThrowsArgumentException()
	{
		// Arrange
		var service = new PlatformStringSizeService();

		// Invalid weight, out of range
		var font = new Font("Arial", 1000, FontStyleType.Normal);

		string testString = "Test";
		float textSize = 32.0f;

		// Act & Assert
		Assert.Throws<ArgumentException>(() =>
		{
			service.GetStringSize(testString, font, textSize);
		});
	}

	[Fact]
	public void GetStringSize_FontWeightWithinRange_DoesNotThrow()
	{
		// Arrange
		var service = new PlatformStringSizeService();

		var font = new Font
		(
			 "Arial",
			 500, // Valid weight, within range
			FontStyleType.Normal
		);
		string testString = "Test";
		float textSize = 32.0f;

		// Act
		var size = service.GetStringSize(testString, font, textSize);

		// Assert
		Assert.True(size.Width > 0);
		Assert.True(size.Height > 0);
	}
}
