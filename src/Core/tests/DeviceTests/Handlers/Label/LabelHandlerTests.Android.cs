using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using ATextAlignemnt = global::Android.Views.TextAlignment;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var labelStub = new LabelStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			var values = await GetValueAsync(labelStub, (handler) =>
			{
				return new
				{
					ViewValue = labelStub.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);

			(var gravity, var textAlignment) = values.PlatformViewValue;

			// Device Tests runner has RTL support enabled, so we expect TextAlignment values
			// (If it didn't, we'd have to fall back to gravity)
			var expectedValue = ATextAlignemnt.ViewEnd;

			Assert.Equal(expectedValue, textAlignment);
		}

		[Fact(DisplayName = "Padding Initializes Correctly")]
		public async Task PaddingInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Padding = new Thickness(5, 10, 15, 20)
			};

			var handler = await CreateHandlerAsync(label);
			var (left, top, right, bottom) = GetNativePadding((TextView)handler.PlatformView);

			var context = handler.PlatformView.Context;

			var expectedLeft = context.ToPixels(5);
			var expectedTop = context.ToPixels(10);
			var expectedRight = context.ToPixels(15);
			var expectedBottom = context.ToPixels(20);

			Assert.Equal(expectedLeft, left);
			Assert.Equal(expectedTop, top);
			Assert.Equal(expectedRight, right);
			Assert.Equal(expectedBottom, bottom);
		}

		[Fact(DisplayName = "TextDecorations Initializes Correctly")]
		public async Task TextDecorationsInitializesCorrectly()
		{
			var xplatTextDecorations = TextDecorations.Underline;

			var labelHandler = new LabelStub()
			{
				TextDecorations = xplatTextDecorations
			};

			var values = await GetValueAsync(labelHandler, (handler) =>
			{
				return new
				{
					ViewValue = labelHandler.TextDecorations,
					PlatformViewValue = GetNativeTextDecorations(handler)
				};
			});

			PaintFlags expectedValue = PaintFlags.UnderlineText;

			Assert.Equal(xplatTextDecorations, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "LineHeight Spaces All Lines Evenly")]
		public async Task LineHeightSpacesAllLinesEvenly()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 3
			};

			var (expectedGap, firstGap, secondGap) = await GetValueAsync(label, handler => GetBaselineGaps(handler));

			// The first line also holds the font's top padding, which must not make the
			// space below it larger than the space below the other lines.
			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Spaces Wrapped Lines Evenly")]
		public async Task LineHeightSpacesWrappedLinesEvenly()
		{
			// One paragraph that wraps, unlike the newline-separated lines above.
			var label = new LabelStub()
			{
				Text = "First line second line third line fourth line",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			var (expectedGap, firstGap, secondGap) = await GetValueAsync(label, handler =>
			{
				// Wide enough for the widest "<word> line" pair but not for three words, so the text
				// wraps into four lines whatever the screen density is
				var paint = GetPlatformLabel(handler).Paint;
				var width = new[] { "First line", "second line", "third line", "fourth line" }.Max(paint.MeasureText);
				return GetBaselineGaps(handler, widthPixels: (int)Math.Ceiling(width));
			});

			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Follows Font Size Changes")]
		public async Task LineHeightFollowsFontSizeChanges()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(10),
				LineHeight = 3
			};

			var handler = await CreateHandlerAsync(label);

			var (expectedGap, _, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				label.Font = Font.SystemFontOfSize(30);
				handler.UpdateValue(nameof(ITextStyle.Font));
				return GetBaselineGaps(handler);
			});

			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Follows Font Changes In Handlers Reusing The Label Mappings")]
		public async Task LineHeightFollowsFontChangesInHandlersReusingTheLabelMappings()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(10),
				LineHeight = 2
			};

			var (fontHeight, extra) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = new LabelMappingsHandler();
				InitializeViewHandler(label, handler);

				label.Font = Font.SystemFontOfSize(30);
				handler.UpdateValue(nameof(ITextStyle.Font));

				var fontMetrics = handler.PlatformView.Paint.GetFontMetricsInt();
				return (fontMetrics.Descent - fontMetrics.Ascent, handler.PlatformView.LineSpacingExtra);
			});

			// LineHeight 2 adds one height of the new font between lines
			Assert.Equal(fontHeight, extra);
		}

		[Fact(DisplayName = "LineHeight Follows Native Text Size Changes")]
		public async Task LineHeightFollowsNativeTextSizeChanges()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(10),
				LineHeight = 3
			};

			var handler = await CreateHandlerAsync(label);

			// Platform customizations, such as a Font mapping appended by the app,
			// can change the native text size after MAUI has mapped LineHeight.
			var (expectedGap, _, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				var textView = GetPlatformLabel(handler);
				textView.SetTextSize(global::Android.Util.ComplexUnitType.Px, textView.TextSize * 3);
				return GetBaselineGaps(handler);
			});

			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Keeps Existing Transformation Method")]
		public async Task LineHeightKeepsExistingTransformationMethod()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			var handler = await CreateHandlerAsync(label);

			// A customized label can install its own transformation, such as all caps.
			var (text, expectedGap, firstGap, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				var textView = GetPlatformLabel(handler);
				textView.SetAllCaps(true);
				label.LineHeight = 3;
				handler.UpdateValue(nameof(ILabel.LineHeight));
				var (expected, first, second) = GetBaselineGaps(handler);
				return (textView.Layout.Text, expected, first, second);
			});

			Assert.Equal("FIRST\nSECOND\nTHIRD", text);
			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Overrides Native Line Spacing")]
		public async Task LineHeightOverridesNativeLineSpacing()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			var handler = await CreateHandlerAsync(label);

			// Spacing set on the native view, for example by a custom handler, must not add to LineHeight.
			var (expectedGap, firstGap, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				GetPlatformLabel(handler).SetLineSpacing(10, 1.5f);
				label.LineHeight = 3;
				handler.UpdateValue(nameof(ILabel.LineHeight));
				return GetBaselineGaps(handler);
			});

			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Keeps Line Spacing Set By An Appended Mapping")]
		public async Task LineHeightKeepsLineSpacingSetByAnAppendedMapping()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			// An app can customize the native line spacing in a mapping that runs after MAUI's.
			var mapper = new PropertyMapper<ILabel, LabelHandler>(LabelHandler.Mapper);
			mapper.AppendToMapping(nameof(ILabel.LineHeight), (handler, view) => handler.PlatformView.SetLineSpacing(20, 1));

			var (extra, multiplier) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = new LabelHandler(mapper);
				InitializeViewHandler(label, handler);
				handler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				return (handler.PlatformView.LineSpacingExtra, handler.PlatformView.LineSpacingMultiplier);
			});

			Assert.Equal(20, extra);
			Assert.Equal(1, multiplier);
		}

		[Fact(DisplayName = "LineHeight Of Zero Keeps The Natural Line Spacing")]
		public async Task LineHeightOfZeroKeepsTheNaturalLineSpacing()
		{
			// iOS and Windows treat 0 as the natural line height; as a multiplier it would stack every line on the first.
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 0
			};

			var (extra, multiplier) = await GetValueAsync(label, handler => (handler.PlatformView.LineSpacingExtra, handler.PlatformView.LineSpacingMultiplier));

			Assert.Equal(0, extra);
			Assert.Equal(1, multiplier);
		}

		[Fact(DisplayName = "LineHeight Scales Html Lines By Their Own Height")]
		public async Task LineHeightScalesHtmlLinesByTheirOwnHeight()
		{
			// Html.FromHtml enlarges the heading with a size span, so its lines are
			// taller than the TextView's base font.
			const string html = "<h1>First<br>Second<br>Third</h1>";

			var (_, _, naturalGap) = await GetHtmlBaselineGaps(html, lineHeight: 1);
			var (_, _, gap) = await GetHtmlBaselineGaps(html, lineHeight: 2);

			// Spanned text keeps the line spacing multiplier, so only the gaps after the
			// first line are compared: they must double with the heading's own height.
			Assert.InRange(gap, 2 * naturalGap - 1, 2 * naturalGap + 1);
		}

		TextView GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		string GetNativeText(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			((uint)GetPlatformLabel(labelHandler).CurrentTextColor).ToColor();

		(GravityFlags gravity, ATextAlignemnt alignment) GetNativeHorizontalTextAlignment(LabelHandler labelHandler)
		{
			var textView = GetPlatformLabel(labelHandler);
			return (textView.Gravity, textView.TextAlignment);
		}

		(double left, double top, double right, double bottom) GetNativePadding(global::Android.Views.View view)
		{
			return (view.PaddingLeft, view.PaddingTop, view.PaddingRight, view.PaddingBottom);
		}

		double GetNativeCharacterSpacing(LabelHandler labelHandler) =>
			Math.Round(GetPlatformLabel(labelHandler).LetterSpacing / UnitExtensions.EmCoefficient, EmCoefficientPrecision);

		PaintFlags GetNativeTextDecorations(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).PaintFlags;

		float GetNativeLineHeight(LabelHandler labelHandler)
		{
			var textView = GetPlatformLabel(labelHandler);
			if (textView.LineSpacingMultiplier != 1)
				return textView.LineSpacingMultiplier;

			var fontMetrics = textView.Paint.GetFontMetricsInt();
			return 1 + textView.LineSpacingExtra / (fontMetrics.Descent - fontMetrics.Ascent);
		}

		// Distances between the first three baselines of a label, plus the distance
		// expected from the font height and the label's LineHeight.
		(int expectedGap, int firstGap, int secondGap) GetBaselineGaps(LabelHandler labelHandler, int widthPixels = 10000)
		{
			// A MAUI layout pass asks the handler for its size first, which refreshes the line spacing.
			// The stubs have an explicit size, so measure the platform view directly afterwards.
			labelHandler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);

			var textView = GetPlatformLabel(labelHandler);
			textView.Measure(
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(widthPixels, MeasureSpecMode.AtMost),
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));

			var layout = textView.Layout;
			Assert.True(layout.LineCount >= 3, $"Expected at least 3 lines, got {layout.LineCount}");

			var fontMetrics = textView.Paint.GetFontMetricsInt();
			var expectedGap = (int)Math.Round((fontMetrics.Descent - fontMetrics.Ascent) * labelHandler.VirtualView.LineHeight);

			return (expectedGap,
				layout.GetLineBaseline(1) - layout.GetLineBaseline(0),
				layout.GetLineBaseline(2) - layout.GetLineBaseline(1));
		}

		Task<(int expectedGap, int firstGap, int secondGap)> GetHtmlBaselineGaps(string html, double lineHeight)
		{
			var label = new LabelStub() { Text = html, LineHeight = lineHeight };

			return GetValueAsync(label, handler =>
			{
				GetPlatformLabel(handler).UpdateTextHtml(label);
				return GetBaselineGaps(handler);
			});
		}

		const string SlantedText = "Hello, World";

		[Theory(DisplayName = "Slanted Last Glyph Is Drawn Completely")]
		[InlineData(false, GravityFlags.Start, null)]
		[InlineData(false, GravityFlags.CenterHorizontal, null)]
		[InlineData(false, GravityFlags.End, null)]
		[InlineData(true, GravityFlags.Start, null)]
		[InlineData(true, GravityFlags.CenterHorizontal, null)]
		[InlineData(true, GravityFlags.End, null)]
		[InlineData(false, GravityFlags.End, "Start")]
		[InlineData(false, GravityFlags.End, "Middle")]
		public async Task SlantedLastGlyphIsDrawnCompletely(bool material, GravityFlags gravity, string truncation)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var textView = CreateSlantedTextView(material);
				textView.Gravity = gravity;

				var width = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
				if (truncation is not null)
				{
					textView.SetMaxLines(1);
					textView.Ellipsize = truncation == "Start" ? global::Android.Text.TextUtils.TruncateAt.Start : global::Android.Text.TextUtils.TruncateAt.Middle;
					var narrow = (int)(textView.Paint.MeasureText(SlantedText) * 0.7f);
					width = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(narrow, MeasureSpecMode.Exactly);
				}

				textView.Measure(width, global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
				textView.Layout(0, 0, textView.MeasuredWidth, textView.MeasuredHeight);

				AssertInkIsNotClipped(textView);
			});
		}

		[Theory(DisplayName = "Slanted Text Keeps Its Line Breaks")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task SlantedTextKeepsItsLineBreaks(bool material)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				const string text = "all of the old wood fell to the floor of the hall and we all had a good look at it";
				var textView = CreateSlantedTextView(material);
				TextView reference = material
					? new global::Google.Android.Material.TextView.MaterialTextView(textView.Context)
					: new AppCompatTextView(MauiContext.Context);
				reference.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 32);
				reference.Paint.TextSkewX = -0.25f;
				reference.Typeface = textView.Typeface;
				textView.Text = text;
				reference.Text = text;

				var height = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
				for (var pixels = 300; pixels <= 900; pixels += 7)
				{
					var width = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(pixels, MeasureSpecMode.Exactly);
					textView.Measure(width, height);
					reference.Measure(width, height);

					Assert.Equal(GetLineStarts(reference), GetLineStarts(textView));
				}
			});
		}

		[Fact(DisplayName = "Narrowed Wrapped Slanted Label Is Drawn Completely")]
		public async Task NarrowedWrappedSlantedLabelIsDrawnCompletely()
		{
			var label = new LabelStub
			{
				Text = "Hello World Hello World Hello World",
				HorizontalLayoutAlignment = Primitives.LayoutAlignment.Start
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(label);
				var textView = (TextView)handler.PlatformView;
				textView.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 32);
				textView.Paint.TextSkewX = -0.25f;

				var size = handler.GetDesiredSize(200, double.PositiveInfinity);
				handler.PlatformArrange(new Graphics.Rect(0, 0, size.Width, size.Height));

				Assert.True(textView.LineCount > 1);
				AssertInkIsNotClipped(textView);
			});
		}

		TextView CreateSlantedTextView(bool material)
		{
			TextView textView = material ? new MauiMaterialTextView(MauiContext.Context) : new MauiTextView(MauiContext.Context);
			textView.Text = SlantedText;
			textView.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 32);
			textView.Paint.TextSkewX = -0.25f;
			return textView;
		}

		static int[] GetLineStarts(TextView textView) =>
			Enumerable.Range(0, textView.LineCount).Select(textView.Layout.GetLineStart).ToArray();

		static void AssertInkIsNotClipped(TextView textView)
		{
			textView.SetTextColor(global::Android.Graphics.Color.Black);

			using var drawn = global::Android.Graphics.Bitmap.CreateBitmap(textView.Width, textView.Height, global::Android.Graphics.Bitmap.Config.Argb8888);
			using var drawnCanvas = new Canvas(drawn);
			textView.Draw(drawnCanvas);

			using var unclipped = global::Android.Graphics.Bitmap.CreateBitmap(textView.Width + 100, textView.Height, global::Android.Graphics.Bitmap.Config.Argb8888);
			using var unclippedCanvas = new Canvas(unclipped);
			unclippedCanvas.Translate(textView.TotalPaddingLeft, textView.TotalPaddingTop);
			textView.Layout.Draw(unclippedCanvas);

			Assert.True(GetRightmostInk(drawn) >= 0, "Nothing was drawn");
			Assert.InRange(GetRightmostInk(drawn), GetRightmostInk(unclipped) - 1, GetRightmostInk(unclipped) + 1);
		}

		static int GetRightmostInk(global::Android.Graphics.Bitmap bitmap)
		{
			var pixels = new int[bitmap.Width * bitmap.Height];
			bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);

			for (var x = bitmap.Width - 1; x >= 0; x--)
			{
				for (var y = 0; y < bitmap.Height; y++)
				{
					if (global::Android.Graphics.Color.GetAlphaComponent(pixels[y * bitmap.Width + x]) > 64)
						return x;
				}
			}

			return -1;
		}

		// An ILabelHandler that reuses the LabelHandler mappings without deriving from LabelHandler
		class LabelMappingsHandler : ViewHandler<ILabel, AppCompatTextView>, ILabelHandler
		{
			static readonly PropertyMapper<ILabel, ILabelHandler> LabelMappings = new(ViewMapper)
			{
				[nameof(ITextStyle.Font)] = LabelHandler.MapFont,
				[nameof(ILabel.LineHeight)] = LabelHandler.MapLineHeight,
				[nameof(ILabel.Text)] = LabelHandler.MapText,
			};

			public LabelMappingsHandler() : base(LabelMappings)
			{
			}

			protected override AppCompatTextView CreatePlatformView() => new(Context);

			ILabel ILabelHandler.VirtualView => VirtualView;

			AppCompatTextView ILabelHandler.PlatformView => PlatformView;
		}
	}
}