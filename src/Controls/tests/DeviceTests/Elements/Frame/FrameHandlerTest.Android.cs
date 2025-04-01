#pragma warning disable CS0618 // Type or member is obsolete
using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui.Controls;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameHandlerTest
	{
		public override async Task ReturnsNonEmptyNativeBoundingBox(int size)
		{
			// Frames have a legacy hard-coded minimum size of 20x20
			var expectedSize = Math.Max(20, size);
			var expectedBounds = new Graphics.Rect(0, 0, expectedSize, expectedSize);

			var view = new Frame()
			{
				HeightRequest = size,
				WidthRequest = size
			};

			var nativeBoundingBox = await GetValueAsync(view, handler => GetBoundingBox(handler));
			Assert.NotEqual(nativeBoundingBox, Graphics.Rect.Zero);

			AssertWithinTolerance(expectedBounds.Size, nativeBoundingBox.Size);
		}
	}
}
#pragma warning restore CS0618 // Type or member is obsolete