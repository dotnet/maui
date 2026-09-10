using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public class Issue33315 : global::Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
	{
		[Fact]
		[Category("Issue33315")]
		public async System.Threading.Tasks.Task ItalicLabelDoesNotClipFinalCharacter()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<global::Microsoft.Maui.Controls.Label, global::Microsoft.Maui.Handlers.LabelHandler>();
				handlers.AddHandler<global::Microsoft.Maui.Controls.Layout, global::Microsoft.Maui.Handlers.LayoutHandler>();
				handlers.AddHandler<global::Microsoft.Maui.Controls.ScrollView, global::Microsoft.Maui.Handlers.ScrollViewHandler>();
				handlers.AddHandler<global::Microsoft.Maui.Controls.Window, global::Microsoft.Maui.DeviceTests.Stubs.WindowHandlerStub>();
			}));
			var headlineStyle = new Style(typeof(Label))
			{
				Setters =
				{
					new Setter { Property = Label.TextColorProperty, Value = global::Microsoft.Maui.Graphics.Color.FromArgb("#190649") },
					new Setter { Property = Label.FontSizeProperty, Value = 32d },
					new Setter { Property = View.HorizontalOptionsProperty, Value = global::Microsoft.Maui.Controls.LayoutOptions.Center },
					new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center }
				}
			};
			var affectedLabel = new Label
			{
				Text = "Hello, World",
				AutomationId = "AffectedLabel",
				BackgroundColor = global::Microsoft.Maui.Graphics.Colors.Orange,
				Style = headlineStyle
			};
			global::Microsoft.Maui.Controls.SemanticProperties.SetHeadingLevel(affectedLabel, SemanticHeadingLevel.Level1);
			var layout = new VerticalStackLayout
			{
				Padding = new Thickness(30, 40),
				Spacing = 25,
				Children = { affectedLabel }
			};
			var scrollView = new ScrollView
			{
				Content = layout
			};
			var applyReportedTrigger = true;
			if (applyReportedTrigger)
			{
				affectedLabel.FontAttributes = FontAttributes.Italic;
			}
			await CreateHandlerAndAddToWindow<global::Microsoft.Maui.DeviceTests.Stubs.WindowHandlerStub>(
				new Window(new ContentPage { Content = scrollView }),
				async _ =>
				{
					await AssertEventually(() => affectedLabel.Handler != null && affectedLabel.IsLoaded);
					Assert.True(affectedLabel.Width > 0 && affectedLabel.Width <= 1024 && affectedLabel.Height > 0 && affectedLabel.Height <= 512);
					var bitmap = await global::Microsoft.Maui.DeviceTests.ImageAnalysis.RawBitmapExtensions.AsRawBitmapAsync(affectedLabel);
					Assert.True(bitmap.PixelWidth >= 4 && bitmap.PixelWidth <= 4096 && bitmap.PixelHeight >= 3 && bitmap.PixelHeight <= 2048);
					Assert.Equal(bitmap.PixelWidth * bitmap.PixelHeight * 4, bitmap.PixelBuffer.Length);
					var backgroundBlue = bitmap.PixelBuffer[0];
					var backgroundGreen = bitmap.PixelBuffer[1];
					var backgroundRed = bitmap.PixelBuffer[2];
					var hasRightEdgeInk = false;
					var hasInteriorInk = false;
					for (var x = 1; x < bitmap.PixelWidth; x++)
					{
						for (var y = 1; y < bitmap.PixelHeight - 1; y++)
						{
							var offset = ((y * bitmap.PixelWidth) + x) * 4;
							var delta = global::System.Math.Abs(bitmap.PixelBuffer[offset] - backgroundBlue) + global::System.Math.Abs(bitmap.PixelBuffer[offset + 1] - backgroundGreen) + global::System.Math.Abs(bitmap.PixelBuffer[offset + 2] - backgroundRed);
							if (delta > 80)
							{
								if (x >= bitmap.PixelWidth - 2)
								{
									hasRightEdgeInk = true;
								}
								else
								{
									hasInteriorInk = true;
								}
							}
						}
					}
					Assert.True(hasInteriorInk);
					Assert.False(hasRightEdgeInk);
				});
		}
	}
}

