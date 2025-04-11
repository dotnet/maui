using Microsoft.Maui.Graphics.Platform;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class PlatformStringSizeServiceTests
{
#if WINDOWS
	[Theory]
	[InlineData(0, 1)]    // Edge case: 0 should become 1
	[InlineData(1, 1)]    // Normal case: 1 stays 1
	[InlineData(500, 500)] // Middle value stays the same
	[InlineData(999, 999)] // Upper bound stays the same
	[InlineData(1000, 999)] // Edge case: 1000 should become 999
	public void GetStringSize_FontWeight_ShouldApplyCorrectWeightLimits(int inputWeight, int expectedWeight)
	{
		// Arrange
		var service = new PlatformStringSizeService();
		var font = new Font("Arial", inputWeight, FontStyleType.Normal);
		float textSize = 32.0f;
    
		// Act
		var canvasTextFormat = service.CreateCanvasTextFormat(font, textSize);
    
		// Assert
		Assert.Equal(expectedWeight, canvasTextFormat.FontWeight.Weight);
	}
#endif
}
