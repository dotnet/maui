using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{

		[Fact(DisplayName = "Using TailTruncation LineBreakMode changes MaxLines")]
		public async Task UsingTailTruncationSetMaxLines()
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				LineBreakMode = LineBreakMode.TailTruncation,
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(1, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.TailTruncation.ToPlatform(), GetPlatformLineBreakMode(handler));
			}));
		}

		UILabel GetPlatformLabel(LabelHandler labelHandler) =>
			(UILabel)labelHandler.PlatformView;

		UILineBreakMode GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).LineBreakMode;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
 			(int)GetPlatformLabel(labelHandler).Lines;

		double GetPlatformCharacterSpacing(LabelHandler labelHandler)
		{
			var attributedText = GetPlatformLabel(labelHandler).AttributedText;
			var characterSpacing = attributedText.GetCharacterSpacing();
			return characterSpacing;
		}

		double GetPlatformLineHeight(LabelHandler labelHandler)
		{
			var attributedText = GetPlatformLabel(labelHandler).AttributedText;
			var lineHeight = attributedText.GetLineHeight();
			return lineHeight;
		}

		TextDecorations GetPlatformTextDecorations(LabelHandler labelHandler)
		{
			var attributedText = GetPlatformLabel(labelHandler).AttributedText;
			var textDecorations = attributedText.GetTextDecorations();
			return textDecorations;
		}

		public static IEnumerable<object[]> GetCharacterSpacingWithLineHeightWithTextDecorationsWorksTestData()
		{
			var label1 = new Label()
			{
				Text = "This is label tests.",
				CharacterSpacing = 5d,
				LineHeight = 1.5d,
				TextDecorations = TextDecorations.Underline
			};

			var label2 = new Label()
			{
				Text = "This is label tests.",
				CharacterSpacing = 5d,
				LineHeight = 1.5d,
				TextDecorations = TextDecorations.Strikethrough
			};

			var label3 = new Label()
			{
				FormattedText = new FormattedString(),
				CharacterSpacing = 5d,
				LineHeight = 1.5d,
				TextDecorations = TextDecorations.Underline
			};
			label3.AddLogicalChild(
				new Span()
				{
					Text = "This is label tests.",
					CharacterSpacing = 5d,
					LineHeight = 1.5d,
					TextDecorations = TextDecorations.Underline
				}
			);

			var label4 = new Label()
			{
				TextType = TextType.Html,
				Text = "<h1>This is label tests.</h1>",
				CharacterSpacing = 5d,
				LineHeight = 1.5d,
				TextDecorations = TextDecorations.Underline
			};

			return new List<object[]>()
			{
				new object[] { label1, 5d, 1.5d, TextDecorations.Underline },
				new object[] { label2, 5d, 1.5d, TextDecorations.Strikethrough },
				new object[] { label3, 0d, 0d, TextDecorations.None },
				new object[] { label4, 0d, 0d, TextDecorations.None }
			};
		}

		[Theory(DisplayName = "Using CharacterSpacing with LineHeight and TextDecorations works Correctly")]
		[MemberData(nameof(GetCharacterSpacingWithLineHeightWithTextDecorationsWorksTestData))]
		public async Task CharacterSpacingWithLineHeightWithTextDecorationsWorksCorrectly(Label label, double expectedCharacterSpacing, double expectedLineHeight, TextDecorations expectedTextDecorations)
		{
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var actualCharacterSpacing = await InvokeOnMainThreadAsync(() => GetPlatformCharacterSpacing(handler));
			Assert.Equal(expectedCharacterSpacing, actualCharacterSpacing);
			var actualLineHeight = await InvokeOnMainThreadAsync(() => GetPlatformLineHeight(handler));
			Assert.Equal(expectedLineHeight, actualLineHeight);
			var actualTextDecorations = await InvokeOnMainThreadAsync(() => GetPlatformTextDecorations(handler));
			Assert.Equal(expectedTextDecorations, actualTextDecorations);
		}
	}
}
