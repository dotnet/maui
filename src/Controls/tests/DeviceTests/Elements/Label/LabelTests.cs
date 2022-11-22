using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
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

				await normalBitmap.AssertEqual(formattedBitmap);
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
		public async Task UpdatingFormattedTextResultsINTheSmaeLayout(double fontSize)
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

				await updatedBitmap.AssertEqual(initialBitmap);
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

				await normalBitmap.AssertEqual(formattedBitmap);
			});
		}
	}
}
