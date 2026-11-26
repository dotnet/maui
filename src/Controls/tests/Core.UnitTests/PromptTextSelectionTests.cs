using System;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PromptTextSelectionTests
	{
		[Theory]
		[InlineData(-1, 0, "start")]
		[InlineData(0, -1, "length")]
		public void NegativeCoordinatesAreRejected(int start, int length, string parameter)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PromptTextSelection(start, length));
			Assert.Equal(parameter, exception.ParamName);
		}

		[Theory]
		[InlineData(0, 6, 10, 0, 6)]
		[InlineData(6, 0, 10, 6, 0)]
		[InlineData(0, 6, 4, 0, 4)]
		[InlineData(6, 0, 4, 4, 0)]
		[InlineData(1, int.MaxValue, 4, 1, 3)]
		[InlineData(int.MaxValue, int.MaxValue, 4, 4, 0)]
		[InlineData(6, 10, 0, 0, 0)]
		public void RangeIsClampedToDisplayedText(int start, int length, int textLength, int expectedStart, int expectedLength)
		{
			Assert.Equal((expectedStart, expectedLength), new PromptTextSelection(start, length).GetRange(textLength));
		}

		[Theory]
		[InlineData("")]
		[InlineData("Repo")]
		[InlineData("Report.pdf")]
		[InlineData("\U0001F600e\u0301")]
		public void AllUsesDisplayedUtf16Length(string displayedText)
		{
			var selection = PromptTextSelection.FromPreset(PromptTextSelection.All);
			Assert.NotNull(selection);
			Assert.Equal((0, displayedText.Length), selection.GetRange(displayedText.Length));
			Assert.NotEqual(new PromptTextSelection(0, 0), selection);
			Assert.Equal(selection, PromptTextSelection.FromPreset(PromptTextSelectionPreset.All));
		}

		[Theory]
		[InlineData("")]
		[InlineData("Repo")]
		[InlineData("Report.pdf")]
		[InlineData("\U0001F600e\u0301")]
		public void NonePlacesCaretAtEndOfDisplayedText(string displayedText)
		{
			var selection = PromptTextSelection.FromPreset(PromptTextSelection.None);
			Assert.True(selection.IsNone);
			Assert.False(selection.IsAll);
			Assert.Equal((displayedText.Length, 0), selection.GetRange(displayedText.Length));
			Assert.Equal(selection, PromptTextSelection.FromPreset(PromptTextSelectionPreset.None));
			Assert.NotEqual(new PromptTextSelection(0, 0), selection);
			Assert.NotEqual(PromptTextSelection.FromPreset(PromptTextSelection.All), selection);
		}

		[Fact]
		public void UndefinedPresetIsRejectedBeforeRequestingPrompt()
		{
			var page = new ContentPage();
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				_ = page.DisplayPromptAsync("Title", "Message", "OK", "Cancel", null, -1, null, "Report.pdf", (PromptTextSelectionPreset)42);
			});
			Assert.Equal("initialSelection", exception.ParamName);
		}

		[Fact]
		public void PromptArgumentsDistinguishUnspecifiedSelectionFromCaretAtStart()
		{
			var original = new PromptArguments("Title", "Message");
			var unspecified = new PromptArguments("Title", "Message", "OK", "Cancel", null, -1, null, "Report.pdf", null);
			var caret = new PromptTextSelection(0, 0);
			var explicitSelection = new PromptArguments("Title", "Message", "OK", "Cancel", null, -1, null, "Report.pdf", caret);

			Assert.Null(original.InitialSelection);
			Assert.Null(unspecified.InitialSelection);
			Assert.Same(caret, explicitSelection.InitialSelection);
			Assert.False(explicitSelection.InitialSelection.IsAll);
		}
	}
}
