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

		[Theory(DisplayName = "Border ContentPanel IsTabStop reflects TapGestureRecognizer state")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ContentPanelIsTabStopReflectsTapGestureRecognizer(bool hasTapGestureRecognizer)
		{
			SetupBuilder();

			var border = new Border()
			{
				Content = new Label
				{
					Text = "Focusable Border"
				},
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100
			};

			if (hasTapGestureRecognizer)
			{
				border.GestureRecognizers.Add(new TapGestureRecognizer());
			}

			await AttachAndRun(border, handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);
				Assert.Equal(hasTapGestureRecognizer, contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop is false when Border is disabled")]
		public async Task ContentPanelIsTabStopFalseWhenBorderIsDisabled()
		{
			SetupBuilder();

			var border = new Border()
			{
				Content = new Label
				{
					Text = "Disabled Border"
				},
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100,
				IsEnabled = false
			};

			border.GestureRecognizers.Add(new TapGestureRecognizer());

			await AttachAndRun(border, handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);
				Assert.False(contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop resets to false when TapGestureRecognizer is removed at runtime")]
		public async Task ContentPanelIsTabStopResetsOnRuntimeRecognizerRemoval()
		{
			SetupBuilder();

			var tapRecognizer = new TapGestureRecognizer();
			var border = new Border()
			{
				Content = new Label
				{
					Text = "Border"
				},
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100
			};

			border.GestureRecognizers.Add(tapRecognizer);

			await AttachAndRun(border, async handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);
				Assert.True(contentPanel.IsTabStop);

				await InvokeOnMainThreadAsync(() => border.GestureRecognizers.Remove(tapRecognizer));

				Assert.False(contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop updates when TapGestureRecognizer Buttons changes at runtime")]
		public async Task ContentPanelIsTabStopUpdatesOnRecognizerButtonsChange()
		{
			SetupBuilder();

			var tapRecognizer = new TapGestureRecognizer { Buttons = ButtonsMask.Primary };
			var border = new Border()
			{
				Content = new Label { Text = "Border" },
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100
			};

			border.GestureRecognizers.Add(tapRecognizer);

			await AttachAndRun(border, async handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);

				// Primary single-tap → should be keyboard-focusable
				Assert.True(contentPanel.IsTabStop);

				// Change to Secondary-only → no longer keyboard-actionable
				await InvokeOnMainThreadAsync(() => tapRecognizer.Buttons = ButtonsMask.Secondary);
				Assert.False(contentPanel.IsTabStop);

				// Restore to Primary → keyboard-focusable again
				await InvokeOnMainThreadAsync(() => tapRecognizer.Buttons = ButtonsMask.Primary);
				Assert.True(contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop updates when IsEnabled toggles at runtime")]
		public async Task ContentPanelIsTabStopUpdatesOnRuntimeIsEnabledToggle()
		{
			SetupBuilder();

			var border = new Border()
			{
				Content = new Label { Text = "Border" },
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100,
				IsEnabled = true
			};

			border.GestureRecognizers.Add(new TapGestureRecognizer());

			await AttachAndRun(border, async handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);

				// Enabled + has recognizer → should be keyboard-focusable
				Assert.True(contentPanel.IsTabStop);

				// Disable at runtime → must leave the tab order
				await InvokeOnMainThreadAsync(() => border.IsEnabled = false);
				Assert.False(contentPanel.IsTabStop);

				// Re-enable at runtime → must re-enter the tab order
				await InvokeOnMainThreadAsync(() => border.IsEnabled = true);
				Assert.True(contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop is false when Border is InputTransparent")]
		public async Task ContentPanelIsTabStopFalseWhenBorderIsInputTransparent()
		{
			SetupBuilder();

			var border = new Border()
			{
				Content = new Label { Text = "InputTransparent Border" },
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100,
				InputTransparent = true
			};

			border.GestureRecognizers.Add(new TapGestureRecognizer());

			await AttachAndRun(border, handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);
				// InputTransparent=true must keep Border out of the tab order even with a recognizer present.
				Assert.False(contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop updates when InputTransparent toggles at runtime")]
		public async Task ContentPanelIsTabStopUpdatesOnRuntimeInputTransparentToggle()
		{
			SetupBuilder();

			var border = new Border()
			{
				Content = new Label { Text = "Border" },
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100,
				InputTransparent = false
			};

			border.GestureRecognizers.Add(new TapGestureRecognizer());

			await AttachAndRun(border, async handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);

				// InputTransparent=false + has recognizer → should be keyboard-focusable
				Assert.True(contentPanel.IsTabStop);

				// Set InputTransparent=true at runtime → must leave the tab order
				await InvokeOnMainThreadAsync(() => border.InputTransparent = true);
				Assert.False(contentPanel.IsTabStop);

				// Restore InputTransparent=false at runtime → must re-enter the tab order
				await InvokeOnMainThreadAsync(() => border.InputTransparent = false);
				Assert.True(contentPanel.IsTabStop);
			});
		}

		[Fact(DisplayName = "Border ContentPanel IsTabStop is false when TapGestureRecognizer is only on a child element")]
		public async Task ContentPanelIsTabStopFalseWhenGestureIsOnChild()
		{
			SetupBuilder();

			var childLabel = new Label { Text = "Child with gesture" };
			childLabel.GestureRecognizers.Add(new TapGestureRecognizer());

			var border = new Border()
			{
				Content = childLabel,
				StrokeShape = new Rectangle(),
				WidthRequest = 300,
				HeightRequest = 100
			};

			// Border itself has NO GestureRecognizers — only the child does.
			await AttachAndRun(border, handler =>
			{
				var contentPanel = GetNativeBorder(handler as BorderHandler);
				// Child gestures must not promote the parent Border into the tab order.
				Assert.False(contentPanel.IsTabStop);
			});
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