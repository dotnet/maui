using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class ResizeImageInfoTests
	{
		public class IsVector
		{
			[Theory]
			[InlineData("image.svg", true)]
			[InlineData("IMAGE.svg", true)]
			[InlineData("image.SVG", true)]
			[InlineData("IMAGE.SVG", true)]
			[InlineData("image.jpeg", false)]
			[InlineData("IMAGE.jpeg", false)]
			[InlineData("image.JPEG", false)]
			[InlineData("IMAGE.JPEG", false)]
			[InlineData("image.png", false)]
			[InlineData("IMAGE.png", false)]
			[InlineData("image.PNG", false)]
			[InlineData("IMAGE.PNG", false)]
			public void ReturnsCorrectFolder(string filename, bool isVector)
			{
				var info = new ResizeImageInfo
				{
					Filename = filename
				};

				Assert.Equal(isVector, info.IsVector);
			}

			[Theory]
			[InlineData("image")]
			[InlineData("IMAGE")]
			public void SupportsNoExtension(string filename)
			{
				var info = new ResizeImageInfo
				{
					Filename = filename
				};

				Assert.False(info.IsVector);
			}

			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void DoesNotCrashOnNullOrEmpty(string filename)
			{
				var info = new ResizeImageInfo
				{
					Filename = filename
				};

				Assert.False(info.IsVector);
			}
		}
	}
}
