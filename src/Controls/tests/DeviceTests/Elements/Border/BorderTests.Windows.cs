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
using Microsoft.UI.Xaml;
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
				var clipHost = Assert.IsType<ContentPanelClipHost>(content.Parent);
				Assert.Same(contentPanel, clipHost.Parent);

				var visual = ElementCompositionPreview.GetElementVisual(clipHost);

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
			Assert.Equal(0, label.Frame.X, 2d);
			Assert.Equal(0, label.Frame.Y, 2d);
			Assert.Equal(requestedSize, label.Frame.Width, 2d);
			Assert.Equal(requestedSize, label.Frame.Height, 2d);
		}

		[Theory(DisplayName = "Border clips transformed content to its shape - Issue 17523")]
		[InlineData(1, 0, 0, 0, false)]
		[InlineData(3, 0, 0, 0, false)]
		[InlineData(3, 30, 0, 0, false)]
		[InlineData(3, 0, 20, -15, false)]
		[InlineData(2, 30, 15, -10, false)]
		[InlineData(3, 0, 0, 0, true)]
		public async Task BorderClipsTransformedContentToItsShape(
			double scale,
			double rotation,
			double translationX,
			double translationY,
			bool hasShadow)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			var content = new BoxView
			{
				Color = Colors.Red,
				Rotation = rotation,
				Scale = scale,
				TranslationX = translationX,
				TranslationY = translationY,
			};

			if (hasShadow)
			{
				content.Shadow = new Shadow
				{
					Brush = Colors.Black,
					Offset = new Point(10, 10),
					Radius = 10,
					Opacity = 1,
				};
			}

			var border = new Border
			{
				Content = content,
				Stroke = Colors.Lime,
				StrokeShape = new Ellipse(),
				StrokeThickness = 8,
				HeightRequest = 100,
				WidthRequest = 100,
			};

			var bitmap = await GetRawBitmap(border, typeof(BorderHandler)).WaitAsync(TimeSpan.FromSeconds(5));

			AssertPixelIsTransparent(bitmap, 10, 10);
			AssertPixelIsRed(bitmap, 50, 50);
		}

		[Fact(DisplayName = "Border clips centered transformed content through nested layout - Issue 17523")]
		public async Task BorderClipsCenteredTransformedContentThroughNestedLayout()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			var content = new BoxView
			{
				Color = Colors.Red,
				HeightRequest = 60,
				HorizontalOptions = LayoutOptions.Center,
				Scale = 3,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 60,
			};
			var layout = new Grid { IsClippedToBounds = true };
			layout.Add(content);

			var border = new Border
			{
				Content = layout,
				StrokeShape = new Ellipse(),
				HeightRequest = 100,
				WidthRequest = 100,
			};

			var bitmap = await GetRawBitmap(border, typeof(BorderHandler)).WaitAsync(TimeSpan.FromSeconds(5));

			AssertPixelIsTransparent(bitmap, 10, 10);
			AssertPixelIsRed(bitmap, 50, 50);
		}

		[Fact(DisplayName = "Border renders asynchronously loaded image without outer resize - Issue 17523")]
		public async Task BorderRendersLoadedImageWithoutOuterResize()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Image, ImageHandler>();
				});
			});

			var image = new Image
			{
				Scale = 4,
			};
			var border = new Border
			{
				Content = image,
				StrokeShape = new Ellipse(),
				HeightRequest = 100,
				WidthRequest = 100,
			};

			await AttachAndRun(border, async (BorderHandler handler) =>
			{
				var nativeImage = Assert.IsType<ImageHandler>(image.Handler).PlatformView;
				var imageOpened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				nativeImage.ImageOpened += OnImageOpened;
				image.Source = "red.png";

				try
				{
					await imageOpened.Task.WaitAsync(TimeSpan.FromSeconds(5));
					var timeout = DateTime.UtcNow.AddSeconds(5);
					var renderedRedPixels = 0;

					while (renderedRedPixels == 0 && DateTime.UtcNow < timeout)
					{
						var bitmap = await handler.PlatformView.ToBitmap(MauiContext);
						renderedRedPixels = CountRedPixels(bitmap);

						if (renderedRedPixels == 0)
						{
							await Task.Delay(50);
						}
					}

					Assert.True(renderedRedPixels > 0, "Expected the loaded image to render without resizing the outer window.");
				}
				finally
				{
					nativeImage.ImageOpened -= OnImageOpened;
				}

				void OnImageOpened(object sender, RoutedEventArgs e) =>
					imageOpened.TrySetResult();
			});
		}

		[Fact(DisplayName = "Border rearranges label after FontSize changes without outer resize - Issue 17523")]
		public async Task BorderRearrangesLabelAfterFontSizeChangesWithoutOuterResize()
		{
			SetupBuilder();

			var label = new Label
			{
				FontSize = 40,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = "+",
				TextColor = Colors.Blue,
				VerticalTextAlignment = TextAlignment.Center,
			};
			var border = new Border
			{
				Content = label,
				StrokeShape = new Ellipse(),
				HeightRequest = 100,
				WidthRequest = 100,
			};

			await AttachAndRun(border, async (BorderHandler handler) =>
			{
				var nativeLabel = Assert.IsType<LabelHandler>(label.Handler).PlatformView;
				var initialDesiredHeight = nativeLabel.DesiredSize.Height;
				var initialBitmap = await handler.PlatformView.ToBitmap(MauiContext);
				await initialBitmap.AssertContainsColor(Colors.Blue, tolerance: 0.1);
				var initialBluePixels = CountBluePixels(initialBitmap);

				label.FontSize = 120;
				await AssertionExtensions.AssertEventually(
					() => nativeLabel.FontSize == 120 &&
						nativeLabel.DesiredSize.Height > initialDesiredHeight,
					timeout: 2000);

				var updatedBitmap = await handler.PlatformView.ToBitmap(MauiContext);
				await updatedBitmap.AssertContainsColor(Colors.Blue, tolerance: 0.1);
				var updatedBluePixels = CountBluePixels(updatedBitmap);

				Assert.True(updatedBluePixels > initialBluePixels * 2,
					$"Expected the larger font to render more blue pixels, but the count changed from {initialBluePixels} to {updatedBluePixels}.");
			});
		}

		[Fact(DisplayName = "Border rearranges content after Padding changes without outer resize - Issue 17523")]
		public async Task BorderRearrangesContentAfterPaddingChangesWithoutOuterResize()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			var content = new BoxView { Color = Colors.Red };
			var border = new Border
			{
				Content = content,
				HeightRequest = 100,
				WidthRequest = 100,
			};

			await AttachAndRun(border, async (BorderHandler handler) =>
			{
				var nativeContent = handler.PlatformView.Content;
				var initialBitmap = await handler.PlatformView.ToBitmap(MauiContext);
				var initialRedPixels = CountRedPixels(initialBitmap);

				border.Padding = 20;

				await AssertionExtensions.AssertEventually(
					() => nativeContent.ActualWidth < 70 && nativeContent.ActualHeight < 70,
					timeout: 2000);

				var updatedBitmap = await handler.PlatformView.ToBitmap(MauiContext);
				var updatedRedPixels = CountRedPixels(updatedBitmap);

				Assert.True(updatedRedPixels < initialRedPixels / 2,
					$"Expected Padding to reduce rendered content without an outer resize, but the red pixel count changed from {initialRedPixels} to {updatedRedPixels}.");
			});
		}

		[Fact(DisplayName = "Border refreshes clip host layout when its handler reconnects - Issue 17523")]
		public async Task BorderRefreshesClipHostLayoutWhenHandlerReconnects()
		{
			SetupBuilder();

			var firstBorder = new Border { Content = new Label { Text = "First" } };
			var secondBorder = new Border { Content = new Label { Text = "Second" } };
			var handler = await CreateHandlerAsync<BorderHandler>(firstBorder);

			await InvokeOnMainThreadAsync(() => handler.SetVirtualView(secondBorder));

			var clipHost = Assert.IsType<ContentPanelClipHost>(handler.PlatformView.ContentClipHost);
			Assert.Same(secondBorder, handler.PlatformView.CrossPlatformLayout);
			Assert.Same(secondBorder, clipHost.CrossPlatformLayout);
		}

		[Fact(DisplayName = "Derived Border handler creates the content clip host - Issue 17523")]
		public async Task DerivedBorderHandlerCreatesContentClipHost()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, PlainContentPanelBorderHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			var border = new Border
			{
				Content = new BoxView { Color = Colors.Red, Scale = 3 },
				StrokeShape = new Ellipse(),
				HeightRequest = 100,
				WidthRequest = 100,
			};

			await AttachAndRun(border, (PlainContentPanelBorderHandler handler) =>
			{
				var clipHost = Assert.IsType<ContentPanelClipHost>(handler.PlatformView.ContentClipHost);
				Assert.Same(clipHost, handler.PlatformView.Content.Parent);
				Assert.NotNull(ElementCompositionPreview.GetElementVisual(clipHost).Clip);
			});
		}

		[Fact(DisplayName = "Border removes wrapped content when content changes")]
		public async Task BorderRemovesWrappedContentWhenContentChanges()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			var oldContent = new BoxView { Color = Colors.Red };
			var newContent = new BoxView { Color = Colors.Blue };
			var border = new Border
			{
				Content = oldContent,
				HeightRequest = 100,
				WidthRequest = 100,
			};

			await AttachAndRun(border, (BorderHandler handler) =>
			{
				oldContent.Shadow = new Shadow();
				oldContent.Handler.UpdateValue(nameof(IView.Shadow));
				var oldPlatformRoot = oldContent.ToPlatform();

				border.Content = newContent;
				handler.UpdateValue(nameof(IBorderView.Content));

				Assert.Null(oldPlatformRoot.Parent);
				var newPlatformView = newContent.ToPlatform();
				Assert.NotNull(newPlatformView.Parent);
				Assert.Equal(1, Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(newPlatformView.Parent));
			});
		}

		[Fact(DisplayName = "Border clears native shape and content clip when shape is removed")]
		public async Task BorderClearsNativeShapeAndContentClipWhenShapeIsRemoved()
		{
			SetupBuilder();

			var border = new Border
			{
				Content = new Label { Text = "Content" },
				StrokeShape = new Ellipse(),
				HeightRequest = 100,
				WidthRequest = 100,
			};

			await AttachAndRun(border, (BorderHandler handler) =>
			{
				var content = handler.PlatformView.Content;
				var clipHost = Assert.IsType<ContentPanelClipHost>(content.Parent);
				Assert.Same(handler.PlatformView, clipHost.Parent);
				Assert.NotNull(ElementCompositionPreview.GetElementVisual(clipHost).Clip);
				Assert.NotNull(handler.PlatformView.BorderPath.Data);

				border.StrokeShape = null;
				handler.UpdateValue(nameof(IBorderView.Shape));

				Assert.Null(ElementCompositionPreview.GetElementVisual(clipHost).Clip);
				Assert.Null(handler.PlatformView.BorderPath.Data);
			});
		}

		static void AssertPixelIsTransparent(ImageAnalysis.RawBitmap bitmap, int x, int y)
		{
			var color = GetPixel(bitmap, x, y);
			Assert.Equal(0, color.Alpha);
		}

		static void AssertPixelIsRed(ImageAnalysis.RawBitmap bitmap, int x, int y)
		{
			var color = GetPixel(bitmap, x, y);
			Assert.True(color.Red > 200 && color.Green < 50 && color.Blue < 50,
				$"Expected pixel ({x}, {y}) to be red, but it was ({color.Red}, {color.Green}, {color.Blue}, {color.Alpha}).");
		}

		static int CountBluePixels(Microsoft.Graphics.Canvas.CanvasBitmap bitmap)
		{
			var count = 0;
			var colors = bitmap.GetPixelColors();

			for (int i = 0; i < colors.Length; i++)
			{
				var color = colors[i];
				if (color.B > 180 && color.G < 120 && color.R < 80)
				{
					count++;
				}
			}

			return count;
		}

		static int CountRedPixels(Microsoft.Graphics.Canvas.CanvasBitmap bitmap)
		{
			var count = 0;
			var colors = bitmap.GetPixelColors();

			for (int i = 0; i < colors.Length; i++)
			{
				var color = colors[i];
				if (color.R > 200 && color.G < 50 && color.B < 50)
				{
					count++;
				}
			}

			return count;
		}

		static (byte Red, byte Green, byte Blue, byte Alpha) GetPixel(ImageAnalysis.RawBitmap bitmap, int x, int y)
		{
			var pixelX = (int)(x * bitmap.Density);
			var pixelY = (int)(y * bitmap.Density);
			var offset = (pixelY * bitmap.PixelWidth + pixelX) * 4;
			Assert.InRange(pixelX, 0, bitmap.PixelWidth - 1);
			Assert.InRange(pixelY, 0, bitmap.PixelHeight - 1);
			Assert.InRange(offset + 3, 3, bitmap.PixelBuffer.Length - 1);

			return (
				bitmap.PixelBuffer[offset + 2],
				bitmap.PixelBuffer[offset + 1],
				bitmap.PixelBuffer[offset],
				bitmap.PixelBuffer[offset + 3]);
		}

		ContentPanel GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

		[Fact(DisplayName = "Border is excluded from Control view by default (AutomationId alone does not opt in)")]
		public async Task BorderExcludedFromControlViewByDefault()
		{
			SetupBuilder();

			var border = new Border
			{
				AutomationId = "TestBorder",
				Content = new Label { Text = "Content" },
			};

			await AttachAndRun(border, (BorderHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				var clipHost = Assert.IsType<ContentPanelClipHost>(handler.PlatformView.Content.Parent);

				// Prove the new MauiBorderAutomationPeer is in use, not the default ContentPanel peer.
				Assert.Equal("Border", peer.GetClassName());
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());

				Assert.Equal("TestBorder", peer.GetAutomationId());
				Assert.False(peer.IsControlElement());
				Assert.Null(FrameworkElementAutomationPeer.CreatePeerForElement(clipHost));
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

		sealed class PlainContentPanelBorderHandler : BorderHandler
		{
			protected override ContentPanel CreatePlatformView() =>
				new() { CrossPlatformLayout = VirtualView };
		}

	}
}