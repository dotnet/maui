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
using Microsoft.UI.Xaml.Automation.Peers;
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

		[Fact(DisplayName = "Border with stroke should not inflate measured size when Label content has same explicit dimensions - Issue 19668")]
		public async Task BorderWithStrokeShouldNotInflateMeasuredSizeWhenLabelHasSameExplicitDimensions()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			// The root cause of #19668: when content has the SAME WidthRequest/HeightRequest as the
			// Border, AdjustForExplicitSize re-expands the content's measured size back to its
			// explicit request even after the stroke inset has reduced the available constraint.
			// This inflates MeasureContent's result by StrokeThickness*2, causing the parent to
			// allocate an oversized slot so the border's right/bottom strokes get clipped.
			const double requestedSize = 100;
			const double strokeThickness = 4;

			var label = new Label
			{
				Text = "Hello",
				WidthRequest = requestedSize,
				HeightRequest = requestedSize,
			};

			var border = new Border
			{
				BackgroundColor = Colors.Blue,
				WidthRequest = requestedSize,
				HeightRequest = requestedSize,
				StrokeThickness = strokeThickness,
				Content = label
			};

			// With bug:   desired size = requestedSize + StrokeThickness*2 → bitmap is ~108x108.
			// After fix:  desired size capped at requestedSize → bitmap is 100x100.
			var bitmap = await GetRawBitmap(border, typeof(BorderHandler)).WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(requestedSize, bitmap.Width, 2d);
			Assert.Equal(requestedSize, bitmap.Height, 2d);
		}

		ContentPanel GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

		[Fact(DisplayName = "Border is excluded from Control view by default (AutomationId alone does not opt in)")]
		public async Task BorderExcludedFromControlViewByDefault()
		{
			SetupBuilder();

			var border = new Border { AutomationId = "TestBorder" };

			await AttachAndRun(border, (BorderHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);

				// Prove the new MauiBorderAutomationPeer is in use, not the default ContentPanel peer.
				Assert.Equal("Border", peer.GetClassName());
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());

				Assert.Equal("TestBorder", peer.GetAutomationId());
				Assert.False(peer.IsControlElement());
			});
		}

		[Fact(DisplayName = "Border opts into Control view when SemanticProperties.Description is set")]
		public async Task BorderOptsIntoControlViewWhenDescriptionIsSet()
		{
			SetupBuilder();

			var border = new Border();
			SemanticProperties.SetDescription(border, "Welcome card");

			await AttachAndRun(border, (BorderHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);

				// Prove the new MauiBorderAutomationPeer is in use, not the default ContentPanel peer.
				Assert.Equal("Border", peer.GetClassName());
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());

				Assert.True(peer.IsControlElement());
			});
		}

	}
}