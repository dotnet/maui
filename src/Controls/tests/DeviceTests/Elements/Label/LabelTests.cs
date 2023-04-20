﻿using System.Threading.Tasks;
#if __IOS__
using Foundation;
#endif
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class LabelTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Label() { Text = text, TextTransform = transform };
			var handler = await CreateHandlerAsync<LabelHandler>(control);
			var platformText = await InvokeOnMainThreadAsync(() => TextForHandler(handler));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Label() { Text = text };
			var handler = await CreateHandlerAsync<LabelHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await InvokeOnMainThreadAsync(() => TextForHandler(handler));
			Assert.Equal(expected, platformText);
		}


		[Theory]
		[InlineData("There", TextTransform.None, "There")]
		[InlineData("There", TextTransform.Uppercase, "THERE")]
		[InlineData("There", TextTransform.Lowercase, "there")]
		public async Task FormattedStringSpanTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "Hello" });
			formattedString.Spans.Add(new Span { Text = text, TextTransform = transform });

			var label = new Label { FormattedText = formattedString };

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var platformText = await InvokeOnMainThreadAsync(() => TextForHandler(handler));

			Assert.Equal("Hello" + expected, platformText);
		}

		[Fact]
		public async Task FormattedStringSpanTextTransformOverridesLabelTextTransform()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "HELLO" });
			formattedString.Spans.Add(new Span { Text = "WORLD", TextTransform = TextTransform.None });
			formattedString.Spans.Add(new Span { Text = "new", TextTransform = TextTransform.Uppercase });

			var label = new Label { FormattedText = formattedString, TextTransform = TextTransform.Lowercase };

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var platformText = await InvokeOnMainThreadAsync(() => TextForHandler(handler));

			Assert.Equal("helloWORLDNEW", platformText);
		}


		string TextForHandler(LabelHandler handler)
		{
#if __IOS__
			return handler.PlatformView.AttributedText?.Value;
#elif __ANDROID__
				return handler.PlatformView.TextFormatted.ToString();
#elif WINDOWS
			return handler.PlatformView.Text;
#endif
		}

		[Fact(DisplayName = "LineBreakMode Initializes Correctly")]
		public async Task LineBreakModeInitializesCorrectly()
		{
			var xplatLineBreakMode = LineBreakMode.TailTruncation;

			var label = new Label()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var expectedValue = xplatLineBreakMode.ToPlatform();

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(expectedValue, GetPlatformLineBreakMode(handler));
			}));
		}

#if !WINDOWS
		[Fact(DisplayName = "Single LineBreakMode changes MaxLines")]
		public async Task SingleLineBreakModeChangesMaxLines()
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToPlatform(), GetPlatformLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.HeadTruncation;
				platformLabel.UpdateLineBreakMode(label);

				Assert.Equal(1, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.HeadTruncation.ToPlatform(), GetPlatformLineBreakMode(handler));
			}));
		}

		[Theory(DisplayName = "Unsetting single LineBreakMode resets MaxLines")]
		[InlineData(LineBreakMode.HeadTruncation)]
		[InlineData(LineBreakMode.NoWrap)]
		public async Task UnsettingSingleLineBreakModeResetsMaxLines(LineBreakMode newMode)
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToPlatform(), GetPlatformLineBreakMode(handler));

				label.LineBreakMode = newMode;
				platformLabel.UpdateLineBreakMode(label);

				Assert.Equal(1, GetPlatformMaxLines(handler));
				Assert.Equal(newMode.ToPlatform(), GetPlatformLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.WordWrap;
				platformLabel.UpdateLineBreakMode(label);

				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToPlatform(), GetPlatformLineBreakMode(handler));
			}));
		}
#endif

		[Fact(DisplayName = "LineBreakMode does not affect MaxLines")]
		public async Task LineBreakModeDoesNotAffectMaxLines()
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToPlatform(), GetPlatformLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.CharacterWrap;
				platformLabel.UpdateLineBreakMode(label);

				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.CharacterWrap.ToPlatform(), GetPlatformLineBreakMode(handler));
			}));
		}

		[Fact(DisplayName = "MaxLines Initializes Correctly")]
		public async Task MaxLinesInitializesCorrectly()
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 2
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(label.MaxLines, (int)GetPlatformMaxLines(handler));
			}));
		}

		[Theory(DisplayName = "Negative MaxLines value with wrap is correct")]
#if __IOS__ || WINDOWS
		[InlineData(0)]
#else
		[InlineData(int.MaxValue)]
#endif
		public async Task NegativeMaxValueWithWrapIsCorrect(int expectedLines)
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = -1,
				LineBreakMode = LineBreakMode.WordWrap
			};

			var platformValue = await GetValueAsync<int, LabelHandler>(label, GetPlatformMaxLines);

			Assert.Equal(expectedLines, platformValue);
		}

		[Theory]
		[InlineData(TextAlignment.Center)]
		[InlineData(TextAlignment.Start)]
		[InlineData(TextAlignment.End)]
		public async Task FormattedStringSpanTextHasCorrectLayoutWhenAligned(TextAlignment alignment)
		{
			var formattedLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 50,
				HorizontalTextAlignment = alignment,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "short" },
						new Span { Text = " long second string"}
					}
				},
			};

			var normalLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 50,
				HorizontalTextAlignment = alignment,
				Text = "short long second string"
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var formattedHandler = CreateHandler<LabelHandler>(formattedLabel);
				var formattedBitmap = await formattedHandler.PlatformView.ToBitmap();

				var normalHandler = CreateHandler<LabelHandler>(normalLabel);
				var normalBitmap = await normalHandler.PlatformView.ToBitmap();

				await normalBitmap.AssertEqualAsync(formattedBitmap);
			});
		}

		[Theory(
#if IOS || MACCATALYST
			Skip = "iOS has issues with null graphics contexts."
#endif
		)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(30)]
		public async Task UpdatingFormattedTextResultsInTheSameLayout(double fontSize)
		{
			var initialLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 60,
				FontSize = fontSize,
				FormattedText = GetFormattedString(),
			};

			var updatedLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 60,
				FontSize = fontSize,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var initialHandler = CreateHandler<LabelHandler>(initialLabel);
				var initialBitmap = await initialHandler.PlatformView.ToBitmap();

				var updatedHandler = CreateHandler<LabelHandler>(updatedLabel);

				updatedLabel.FormattedText = GetFormattedString();

				var updatedBitmap = await updatedHandler.PlatformView.ToBitmap();

				await updatedBitmap.AssertEqualAsync(initialBitmap);
			});

			static FormattedString GetFormattedString() =>
				new FormattedString
				{
					Spans =
					{
						new Span { Text = "first" },
						new Span { Text = "\n"},
						new Span { Text = "second"},
					}
				};
		}

		[Theory(
#if ANDROID
			Skip = "Android does not have the exact same layout with a string vs spans."
#endif
		)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(30)]
		public async Task InitialFormattedTextMatchesText(double fontSize)
		{
			var formattedLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 60,
				FontSize = fontSize,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "first" },
						new Span { Text = "\n"},
						new Span { Text = "second"},
					}
				},
			};

			var normalLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 60,
				FontSize = fontSize,
				Text = "first\nsecond"
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var formattedHandler = CreateHandler<LabelHandler>(formattedLabel);
				var formattedBitmap = await formattedHandler.PlatformView.ToBitmap();

				var normalHandler = CreateHandler<LabelHandler>(normalLabel);
				var normalBitmap = await normalHandler.PlatformView.ToBitmap();

				await normalBitmap.AssertEqualAsync(formattedBitmap);
			});
		}

		[Fact]
		public async Task TextColorAppliesEvenInHtmlMode()
		{
			// Note: this is specifically a Controls-level rule that's inherited from Forms
			// There's no reason other SDKs need to force a TextColor property when dealing 
			// with HTML text (since HTML text has its own color handling)

			var label = new Label
			{
				TextType = TextType.Html,
				TextColor = Colors.Red,
				Text = "<p>Test</p>"
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				Assert.Equal(Colors.Red, TextColor(handler));
			});
		}

		[Fact]
		public async Task FontStuffAppliesEvenInHtmlMode()
		{
			// Note: this is specifically a Controls-level rule that's inherited from Forms
			// There's no reason other SDKs need to force font properties when dealing 
			// with HTML text (since HTML can do that on its own)

			var label = new Label
			{
				TextType = TextType.Html,
				FontSize = 64,
				FontFamily = "Baskerville",
				Text = "<p>Test</p>"
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				AssertEquivalentFont(handler, label.ToFont());
			});
		}

		[Theory (
#if WINDOWS
			Skip = "Appears that the layout is not getting rendered on Windows."
#endif
			)]
		[InlineData(TextAlignment.Start, LineBreakMode.HeadTruncation)]
		[InlineData(TextAlignment.Start, LineBreakMode.MiddleTruncation)]
		[InlineData(TextAlignment.Start, LineBreakMode.TailTruncation)]
		[InlineData(TextAlignment.Center, LineBreakMode.HeadTruncation)]
		[InlineData(TextAlignment.Center, LineBreakMode.MiddleTruncation)]
		[InlineData(TextAlignment.Center, LineBreakMode.TailTruncation)]
		[InlineData(TextAlignment.End, LineBreakMode.HeadTruncation)]
		[InlineData(TextAlignment.End, LineBreakMode.MiddleTruncation)]
		[InlineData(TextAlignment.End, LineBreakMode.TailTruncation)]
#if __IOS__
		[InlineData(TextAlignment.Start, LineBreakMode.NoWrap, false)]
		[InlineData(TextAlignment.Center, LineBreakMode.NoWrap, false)]
		[InlineData(TextAlignment.End, LineBreakMode.NoWrap, false)]
#elif __ANDROID__
		[InlineData(TextAlignment.Start, LineBreakMode.NoWrap)]
		[InlineData(TextAlignment.Center, LineBreakMode.NoWrap)]
		[InlineData(TextAlignment.End, LineBreakMode.NoWrap)]
#endif
		public async Task LabelTruncatesCorrectly(TextAlignment textAlignment, LineBreakMode lineBreakMode, bool isEqual = true)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var labelStart = new Label
			{
				HorizontalOptions = LayoutOptions.Start,
				LineBreakMode = lineBreakMode,
				HorizontalTextAlignment = textAlignment,
				Text = LoremIpsum,
			};

			var labelCenter = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				LineBreakMode = lineBreakMode,
				HorizontalTextAlignment = textAlignment,
				Text = LoremIpsum,
			};

			var labelEnd = new Label
			{
				HorizontalOptions = LayoutOptions.End,
				LineBreakMode = lineBreakMode,
				HorizontalTextAlignment = textAlignment,
				Text = LoremIpsum,
			};

			var labelFill = new Label
			{
				HorizontalOptions = LayoutOptions.Fill,
				LineBreakMode = lineBreakMode,
				HorizontalTextAlignment = textAlignment,
				Text = LoremIpsum,
			};

			var layout = new VerticalStackLayout()
				{
					labelStart,
					labelCenter,
					labelEnd,
					labelFill,
				};

			layout.HeightRequest = 300;
			layout.WidthRequest = 100;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(() =>
				{
					if (isEqual)
					{
						Assert.Equal(double.Round(labelStart.Width, 5), double.Round(layout.Width, 5));
						Assert.Equal(double.Round(labelCenter.Width, 5), double.Round(layout.Width, 5));
						Assert.Equal(double.Round(labelEnd.Width, 5), double.Round(layout.Width, 5));
					}
					// with LineBreakMode.NoWrap, we do not expect them to be equal if HorizontalOptions != LayoutOptions.Fill
					else
					{
						Assert.NotEqual(labelStart.Width, layout.Width);
						Assert.NotEqual(labelCenter.Width, layout.Width);
						Assert.NotEqual(labelEnd.Width, layout.Width);
					}

					Assert.Equal(double.Round(labelFill.Width, 5), double.Round(layout.Width, 5));
				});
			});
		}

		static readonly string LoremIpsum = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

		Color TextColor(LabelHandler handler)
		{
#if __IOS__
			return GetPlatformLabel(handler).TextColor.ToColor();
#elif __ANDROID__
			return ((uint)GetPlatformLabel(handler).CurrentTextColor).ToColor();
#elif WINDOWS
			return (GetPlatformLabel(handler).Foreground as UI.Xaml.Media.SolidColorBrush).Color.ToColor();
#endif
		}

		static void AssertEquivalentFont(LabelHandler handler, Font font)
		{
			var fontManager = (IFontManager)handler.MauiContext.Services.GetService(typeof(IFontManager));

#if __IOS__
			var targetTypeface = fontManager.GetFont(font);
			var platformTypeface = handler.PlatformView.AttributedText.GetUIKitAttributes(0, out NSRange range).Font;
			var targetFontSize = fontManager.GetFont(font).PointSize;

			Assert.Equal(targetTypeface, platformTypeface);
			Assert.Equal(targetFontSize, platformTypeface.PointSize);
#elif __ANDROID__
			var targetTypeface = fontManager.GetTypeface(font);
			var targetFontSize = handler.MauiContext.Context.ToPixels(fontManager.GetFontSize(font).Value);		
			var platformTypeface = handler.PlatformView.Typeface;
			var platformFontSize = handler.PlatformView.TextSize;

			Assert.Equal(targetTypeface, platformTypeface);
			Assert.Equal(targetFontSize, platformFontSize);
#elif WINDOWS
			var targetFontStyle = font.ToFontStyle();
			var targetFontWeight = font.ToFontWeight();
			var platformFontStyle = handler.PlatformView.FontStyle;
			var platformFontWeight = handler.PlatformView.FontWeight;

			Assert.Equal(targetFontStyle, platformFontStyle);
			Assert.Equal(targetFontWeight, platformFontWeight);
#endif
		}
	}
}
