using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		[Fact(DisplayName = "Shadow Initializes Correctly")]
		public async Task ShadowInitializesCorrectly()
		{
			var xPlatShadow = new Shadow
			{
				Color = Colors.Red,
				Offset = new Size(10, 10),
				Opacity = 1.0f,
				Radius = 2.0f
			};

			var layout = new LayoutStub
			{
				Height = 50,
				Width = 50
			};

			layout.Shadow = xPlatShadow;

			var expectedNativeShadowColor = xPlatShadow.Color.ToNative();

			var values = await GetValueAsync(layout, (handler) =>
			{
				return new
				{
					ViewValue = layout.Shadow,
					NativeViewValue = GetNativeShadowColor(handler)
				};
			});

			Assert.Equal(xPlatShadow, values.ViewValue);
			Assert.Equal(expectedNativeShadowColor, values.NativeViewValue);
		}

		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return layoutHandler.NativeView.ChildCount;
		}

		AColor GetNativeShadowColor(LayoutHandler layoutHandler)
		{
			var outlineSpotShadowColor = layoutHandler.NativeView.OutlineSpotShadowColor;
			return new AColor(outlineSpotShadowColor);
		}
	}
}