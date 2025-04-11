using Microsoft.Maui.Graphics.Platform;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class PlatformStringSizeServiceTests
{
#if WINDOWS
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(500)]
	[InlineData(999)]
	[InlineData(1000)]
    public void GetStringSize_FontWeight(int fontWeight)
    {
        // Arrange
        var service = new PlatformStringSizeService();
        var font = new Font("Arial", fontWeight, FontStyleType.Normal);

        //string textString = "Test";
        float textSize = 32.0f;

        // Act
        var canvasTextFormat = service.CreateCanvasTextFormat(font, textSize);

        // Assert
		if(fontWeight == 0)
		{
			Assert.Equal(1, canvasTextFormat.FontWeight.Weight);
		}
		else if (fontWeight == 1000)
		{
			Assert.Equal(999, canvasTextFormat.FontWeight.Weight);
		}
		else
		{
			Assert.Equal(fontWeight, canvasTextFormat.FontWeight.Weight);
		}
    }
#endif
}
