using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameTests
	{
		[Fact(DisplayName = "Frame HasShadow Test")]
		public async Task FrameHasShadowTest()
		{
			SetupBuilder();

			var frame = new Frame()
			{
				HasShadow = true,
				HeightRequest = 200,
				WidthRequest = 200,
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "HasShadow"
				}
			};

			await InvokeOnMainThreadAsync(() =>
				frame.ToPlatform(MauiContext).AttachAndRun(async () =>
				{
					var platformView = (Controls.Handlers.Compatibility.FrameRenderer)frame.ToPlatform(MauiContext);
					Assert.NotNull(platformView);

					// The way the shadow is applied in .NET MAUI on iOS is the same way it was applied in Forms
					// so on iOS we just return the shadow that was hard coded into the renderer
					var expectedShadow = new Shadow() { Radius = 5, Opacity = 0.8f, Offset = new Point(0, 0), Brush = Brush.Black };

					if(platformView.Element is IView element)
					{
						var platformShadow = element.Shadow;
						await AssertionExtensions.Wait(() => platformShadow != null);

						Assert.Equal(platformShadow.Radius, expectedShadow.Radius);
						Assert.Equal(platformShadow.Opacity, expectedShadow.Opacity);
						Assert.Equal(platformShadow.Offset, expectedShadow.Offset);
					}
				}));				
		}
	}
}