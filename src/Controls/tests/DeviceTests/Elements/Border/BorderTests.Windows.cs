using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderTests : ControlsHandlerTestBase
	{
		[Theory(DisplayName = "Inner CornerRadius Initializes Correctly")]
		[InlineData(0)]
		[InlineData(12)]
		[InlineData(24)]
		public async Task InnerCornerRadiusInitializesCorrectly(int cornerRadius)
		{
			SetupBuilder();

			var expected = Colors.Red;

			var border = new Border()
			{
				Content = new Label
				{
					Text = "Background",
					TextColor = Colors.Red,
					FontFamily = "Segoe UI",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				StrokeShape = new RoundRectangle { CornerRadius = cornerRadius },
				Background = new SolidPaint(expected),
				StrokeThickness = 0,
				HeightRequest = 100,
				WidthRequest = 300
			};

			await AttachAndRun(border, (handler) =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);
				var content = contentPanel.Content;
				var visual = ElementCompositionPreview.GetElementVisual(content);

				var clip = visual.Clip as CompositionGeometricClip;
				Assert.NotNull(clip);

				var geometry = clip.Geometry as CompositionPathGeometry;
				var path = geometry.Path;
				Assert.NotNull(path);

				Assert.True(contentPanel.IsInnerPath);
			});

			await AssertColorAtPoint(border, expected, typeof(BorderHandler), cornerRadius, cornerRadius);
		}

		[Fact]
		[Description("The IsVisible property of a Border should match with native IsVisible")]
		public async Task VerifyBorderIsVisibleProperty()
		{
			var border = new Border();
			border.IsVisible = false;
			var expectedValue = border.IsVisible;

			var handler = await CreateHandlerAsync<BorderHandler>(border);
			var nativeView = GetNativeBorder(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var isVisible = nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}

		[Fact(DisplayName = "Border should not expand beyond its requested size when BoxView content is larger - Issue 19668")]
		public async Task BorderShouldNotExpandBeyondRequestedSizeWithBoxViewContent()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
	   {
				 handlers.AddHandler<Border, BorderHandler>();
				 handlers.AddHandler<BoxView, BoxViewHandler>();
			 });
			});

			// BoxView is intentionally LARGER than the Border's requested size.
			// The bug: the BoxView pushes the Border to expand beyond its WidthRequest/HeightRequest.
			// The fix: the Border must constrain itself to its requested size regardless of content.
			var boxView = new BoxView
			{
				Color = Colors.Red,
				WidthRequest = 120,
				HeightRequest = 120,
			};

			var border = new Border
			{
				BackgroundColor = Colors.Blue,
				WidthRequest = 80,
				HeightRequest = 80,
				Content = boxView
			};

			// GetRawBitmap dimensions reflect the actual rendered size of the Border.
			// With bug:   Border expands to fit BoxView → bitmap is ~120x120 → assertions FAIL.
			// After fix:  Border stays at requested size → bitmap is 80x80 → assertions PASS.
			var bitmap = await GetRawBitmap(border, typeof(BorderHandler)).WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(80, bitmap.Width, 2d);
			Assert.Equal(80, bitmap.Height, 2d);
		}

		ContentPanel GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

	}
}