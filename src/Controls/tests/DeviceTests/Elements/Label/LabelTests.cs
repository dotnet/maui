using System;
using System.Threading.Tasks;
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
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
				});
			});
		}

		[Fact(DisplayName = "Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();

			WeakReference viewReference = null;
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();

				var label = new Label
				{
					Text = "Test"
				};

				layout.Add(label);
				var handler = CreateHandler<LayoutHandler>(layout);
				viewReference = new WeakReference(label);
				handlerReference = new WeakReference(label.Handler);
				platformViewReference = new WeakReference(label.Handler.PlatformView);
			});

			await AssertionExtensions.WaitForGC(viewReference, handlerReference, platformViewReference);
			Assert.False(viewReference.IsAlive, "Label should not be alive!");
			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformViewReference.IsAlive, "PlatformView should not be alive!");
		}

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
#if IOS || MACCATALYST
			return handler.PlatformView.AttributedText?.Value;
#elif ANDROID
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

		[Fact(DisplayName = "LineBreakMode TailTruncation does not affect MaxLines")]
		public async Task TailTruncationDoesNotAffectMaxLines()
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.TailTruncation,
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.TailTruncation.ToPlatform(), GetPlatformLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.CharacterWrap;
				platformLabel.UpdateLineBreakMode(label);

				Assert.Equal(3, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.CharacterWrap.ToPlatform(), GetPlatformLineBreakMode(handler));
			});
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

		[Fact]
		public async Task ChangingTextTypeWithFormattedTextSwitchesTextSource()
		{
			SetupBuilder();

			Label label;
			var layout = new VerticalStackLayout
			{
				(label = new Label
				{
					WidthRequest = 200,
					HeightRequest = 100,
					BackgroundColor = Colors.Blue,
					FormattedText = new FormattedString
					{
						Spans =
						{
							new Span { Text = "short", TextColor = Colors.Red, FontSize = 20 },
							new Span { Text = " long second string"}
						}
					},
				})
			};

			await AttachAndRun(layout, async (handler) =>
			{
				var platformView = handler.ToPlatform();
				await platformView.AssertContainsColor(Colors.Red, MauiContext);

				label.TextType = TextType.Html;

				await platformView.AssertDoesNotContainColor(Colors.Red, MauiContext);
			});
		}

		[Theory(
#if WINDOWS
		Skip = "Fails on Windows"
#endif
		)]
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
				var formattedBitmap = await formattedHandler.PlatformView.ToBitmap(MauiContext);

				var normalHandler = CreateHandler<LabelHandler>(normalLabel);
				var normalBitmap = await normalHandler.PlatformView.ToBitmap(MauiContext);

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
				var initialBitmap = await initialHandler.PlatformView.ToBitmap(MauiContext);

				var updatedHandler = CreateHandler<LabelHandler>(updatedLabel);

				updatedLabel.FormattedText = GetFormattedString();

				var updatedBitmap = await updatedHandler.PlatformView.ToBitmap(MauiContext);

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
#elif WINDOWS
			Skip = "Fails on Windows"
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
				var formattedBitmap = await formattedHandler.PlatformView.ToBitmap(MauiContext);

				var normalHandler = CreateHandler<LabelHandler>(normalLabel);
				var normalBitmap = await normalHandler.PlatformView.ToBitmap(MauiContext);

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

		[Fact]
		public async Task FormattedStringSpanTextHasCorrectColorWhenChanges()
		{
			var formattedLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 50,
				FontSize = 16,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "short" },
						new Span { Text = " long second string" },
						new Span { Text = " blue string", TextColor = Colors.Blue },
					}
				},
			};

			formattedLabel.TextColor = Colors.Red;

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LabelHandler>(formattedLabel);

				await handler.PlatformView.AssertContainsColor(Colors.Blue, MauiContext);
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);
			});
		}

		[Fact]
		public async Task FormattedStringSpanTextHasCorrectColorWhenChangedAfterCreation()
		{
			var formattedLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 50,
				FontSize = 16,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "short" },
						new Span { Text = " long second string" },
						new Span { Text = " blue string", TextColor = Colors.Blue },
					}
				},
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LabelHandler>(formattedLabel);

				await handler.PlatformView.AssertContainsColor(Colors.Blue, MauiContext);
				await handler.PlatformView.AssertDoesNotContainColor(Colors.Red, MauiContext);

				formattedLabel.TextColor = Colors.Red;

				await handler.PlatformView.AssertContainsColor(Colors.Blue, MauiContext);
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);
			});
		}

		[Theory]
#if !WINDOWS
		// TODO fix these, failing on Windows
		[InlineData(TextAlignment.Start, LineBreakMode.HeadTruncation)]
		[InlineData(TextAlignment.Start, LineBreakMode.MiddleTruncation)]
		[InlineData(TextAlignment.Start, LineBreakMode.TailTruncation)]
		[InlineData(TextAlignment.Center, LineBreakMode.HeadTruncation)]
		[InlineData(TextAlignment.Center, LineBreakMode.MiddleTruncation)]
		[InlineData(TextAlignment.Center, LineBreakMode.TailTruncation)]
		[InlineData(TextAlignment.End, LineBreakMode.HeadTruncation)]
		[InlineData(TextAlignment.End, LineBreakMode.MiddleTruncation)]
		[InlineData(TextAlignment.End, LineBreakMode.TailTruncation)]
#endif
		[InlineData(TextAlignment.Start, LineBreakMode.NoWrap)]
		[InlineData(TextAlignment.Center, LineBreakMode.NoWrap)]
		[InlineData(TextAlignment.End, LineBreakMode.NoWrap)]
		public async Task LabelTruncatesCorrectly(TextAlignment textAlignment, LineBreakMode lineBreakMode)
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

#if WINDOWS
			await AttachAndRun(layout, (handler) =>
			{
				var layoutPlatWidth = handler.PlatformView.Width;
				Assert.Equal(labelStart.Width, layoutPlatWidth);
				Assert.Equal(labelCenter.Width, layoutPlatWidth);
				Assert.Equal(labelEnd.Width, layoutPlatWidth);
				Assert.Equal(double.Round(labelFill.Width, 5), double.Round(layoutPlatWidth, 5));
			});

#else
			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(() =>
				{
					Assert.Equal(double.Round(labelStart.Width, 5), double.Round(layout.Width, 5));
					Assert.Equal(double.Round(labelCenter.Width, 5), double.Round(layout.Width, 5));
					Assert.Equal(double.Round(labelEnd.Width, 5), double.Round(layout.Width, 5));
					Assert.Equal(double.Round(labelFill.Width, 5), double.Round(layout.Width, 5));
				});
			});
#endif
		}

		static readonly string LoremIpsum = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

		[Fact]
		public async Task TextTypeAfterFontStuffIsCorrect()
		{
			// Note: this is specifically a Controls-level rule that's inherited from Forms
			// There's no reason other SDKs need to force font properties when dealing 
			// with HTML text (since HTML can do that on its own)

			var label = new Label
			{
				FontSize = 64,
				FontFamily = "Baskerville",
				Text = "<p>Test</p>"
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				label.TextType = TextType.Html;
				AssertEquivalentFont(handler, label.ToFont());
			});
		}

		[Fact]
		public async Task FontStuffAfterTextTypeIsCorrect()
		{
			// Note: this is specifically a Controls-level rule that's inherited from Forms
			// There's no reason other SDKs need to force font properties when dealing 
			// with HTML text (since HTML can do that on its own)

			var label = new Label
			{
				TextType = TextType.Html,
				Text = "<p>Test</p>"
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				label.FontFamily = "Baskerville";
				label.FontSize = 64;
				AssertEquivalentFont(handler, label.ToFont());
			});
		}

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
