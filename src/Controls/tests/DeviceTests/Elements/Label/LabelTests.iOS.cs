using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
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

		[Fact(DisplayName = "Using CharacterSpacing with LineHeight and TextDecorations works Correctly")]
		public async Task CharacterSpacingWithLineHeightWithTextDecorationsWorksCorrectly()
		{
			var expectedCharacterSpacing1 = 5d;
			var expectedCharacterSpacing2 = 5d;
			var expectedCharacterSpacing3 = 0d;
			var expectedCharacterSpacing4 = 0d;
			var expectedLineHeight1 = 1.5d;
			var expectedLineHeight2 = 1.5d;
			var expectedLineHeight3 = 0d;
			var expectedLineHeight4 = 0d;
			var expectedTextDecorations1 = TextDecorations.Underline;
			var expectedTextDecorations2 = TextDecorations.Strikethrough;
			var expectedTextDecorations3 = TextDecorations.None;
			var expectedTextDecorations4 = TextDecorations.None;

			var label1 = new Label()
			{
				Text = "This is label tests.",
				CharacterSpacing = expectedCharacterSpacing1,
				LineHeight = expectedLineHeight1,
				TextDecorations = expectedTextDecorations1
			};

			var label2 = new Label()
			{
				Text = "This is label tests.",
				CharacterSpacing = expectedCharacterSpacing2,
				LineHeight = expectedLineHeight2,
				TextDecorations = expectedTextDecorations2
			};

			var label3 = new Label()
			{
				FormattedText = new FormattedString(),
				CharacterSpacing = 5d,
				LineHeight = 1.5d,
				TextDecorations = TextDecorations.Underline
			};
			label3.FormattedText.AddLogicalChild(
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

			var handler1 = await CreateHandlerAsync<LabelHandler>(label1);
			var actualCharacterSpacing1 = await InvokeOnMainThreadAsync(() => GetPlatformCharacterSpacing(handler1));
			Assert.Equal(expectedCharacterSpacing1, actualCharacterSpacing1);
			var actualLineHeight1 = await InvokeOnMainThreadAsync(() => GetPlatformLineHeight(handler1));
			Assert.Equal(expectedLineHeight1, actualLineHeight1);
			var actualTextDecorations1 = await InvokeOnMainThreadAsync(() => GetPlatformTextDecorations(handler1));
			Assert.Equal(expectedTextDecorations1, actualTextDecorations1);

			var handler2 = await CreateHandlerAsync<LabelHandler>(label2);
			var actualCharacterSpacing2 = await InvokeOnMainThreadAsync(() => GetPlatformCharacterSpacing(handler2));
			Assert.Equal(expectedCharacterSpacing2, actualCharacterSpacing2);
			var actualLineHeight2 = await InvokeOnMainThreadAsync(() => GetPlatformLineHeight(handler2));
			Assert.Equal(expectedLineHeight2, actualLineHeight2);
			var actualTextDecorations2 = await InvokeOnMainThreadAsync(() => GetPlatformTextDecorations(handler2));
			Assert.Equal(expectedTextDecorations2, actualTextDecorations2);

			var handler3 = await CreateHandlerAsync<LabelHandler>(label3);
			var actualCharacterSpacing3 = await InvokeOnMainThreadAsync(() => GetPlatformCharacterSpacing(handler3));
			Assert.Equal(expectedCharacterSpacing3, actualCharacterSpacing3);
			var actualLineHeight3 = await InvokeOnMainThreadAsync(() => GetPlatformLineHeight(handler3));
			Assert.Equal(expectedLineHeight3, actualLineHeight3);
			var actualTextDecorations3 = await InvokeOnMainThreadAsync(() => GetPlatformTextDecorations(handler3));
			Assert.Equal(expectedTextDecorations3, actualTextDecorations3);

			var handler4 = await CreateHandlerAsync<LabelHandler>(label4);
			var actualCharacterSpacing4 = await InvokeOnMainThreadAsync(() => GetPlatformCharacterSpacing(handler4));
			Assert.Equal(expectedCharacterSpacing4, actualCharacterSpacing4);
			var actualLineHeight4 = await InvokeOnMainThreadAsync(() => GetPlatformLineHeight(handler4));
			Assert.Equal(expectedLineHeight4, actualLineHeight4);
			var actualTextDecorations4 = await InvokeOnMainThreadAsync(() => GetPlatformTextDecorations(handler4));
			Assert.Equal(expectedTextDecorations4, actualTextDecorations4);
		}
	}
}
