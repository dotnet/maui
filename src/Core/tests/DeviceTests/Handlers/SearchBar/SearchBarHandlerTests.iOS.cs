using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		// Regression tests for https://github.com/dotnet/maui/issues/30779
		// SearchBar.CursorPosition and SelectionLength were not updated when the user typed

		[Fact(DisplayName = "CursorPosition Updates When SearchBar Gains Focus (Issue 30779)")]
		public async Task CursorPositionUpdatesWhenSearchBarGainsFocus()
		{
			// The actual issue #30779 scenario: user taps SearchBar to focus it,
			// which causes UIKit to position cursor at end of existing text.
			// DidChangeSelection fires → CursorPosition should update to text.Length.
			// Before the fix (no DidChangeSelection handler), CursorPosition stayed at 0.
			var searchBar = new SearchBarStub { Text = "Hello" };

			await AttachAndRun(searchBar, (handler) =>
			{
				var control = handler.QueryEditor;
				// Simulate user tapping the SearchBar to focus it
				control.BecomeFirstResponder();

				// On iOS, BecomeFirstResponder auto-positions cursor at end and fires DidChangeSelection.
				// On Mac Catalyst, the focus model differs and DidChangeSelection may not fire automatically.
				// Explicitly set cursor to end of text to simulate user tap behavior on all platforms.
				var endPos = control.GetPosition(control.BeginningOfDocument, 5);
				control.SelectedTextRange = control.GetTextRange(endPos, endPos);

				// OnSelectionChanged should update CursorPosition via DidChangeSelection
				Assert.Equal(5, searchBar.CursorPosition);
			});

			// Cursor should be at position 5 (end of "Hello") after focus
			Assert.Equal(5, searchBar.CursorPosition);
		}

		[Fact(DisplayName = "CursorPosition Does Not Change When Text Set Programmatically Without Focus (Issue 30779)")]
		public async Task CursorPositionDoesNotChangeWhenTextSetProgrammaticallyWithoutFocus()
		{
			// Verifies the CORRECT behavior: programmatic UISearchBar.Text change on an
			// unfocused SearchBar must NOT update CursorPosition. The user may have
			// intentionally positioned the cursor somewhere; a background data-binding
			// update should not silently reset it.
			var searchBar = new SearchBarStub();
			// CursorPosition default is 0; it must remain 0 after programmatic text set

			await AttachAndRun(searchBar, (handler) =>
			{
				// Set text programmatically — SearchBar is NOT focused (not first responder)
				GetNativeSearchBar(handler).Text = "Hello";

				// Cursor position must stay at 0; it should NOT jump to 5
				Assert.Equal(0, searchBar.CursorPosition);
			});

			Assert.Equal(0, searchBar.CursorPosition);
		}

		[Fact(DisplayName = "SelectionLength Updates When Text Is Selected Natively (Issue 30779)")]
		public async Task SelectionLengthUpdatesWhenTextIsSelectedNatively()
		{
			var searchBar = new SearchBarStub { Text = "Hello World" };
			int virtualSelectionLength = -1;
			int virtualCursorPosition = -1;

			await AttachAndRun(searchBar, (handler) =>
			{
				var control = handler.QueryEditor;
				// Must be focused: OnSelectionChanged only updates cursor/selection when IsFirstResponder.
				control.BecomeFirstResponder();

				// Select the word "Hello" (chars 0–5)
				var startPosition = control.GetPosition(control.BeginningOfDocument, 0);
				var endPosition = control.GetPosition(control.BeginningOfDocument, 5);
				// SelectedTextRange assignment fires DidChangeSelection synchronously
				control.SelectedTextRange = control.GetTextRange(startPosition, endPosition);

				virtualSelectionLength = searchBar.SelectionLength;
				virtualCursorPosition = searchBar.CursorPosition;
			});

			// Selection of 5 characters starting at position 0 should be reflected in the VirtualView
			Assert.Equal(0, virtualCursorPosition);
			Assert.Equal(5, virtualSelectionLength);
		}

		[Fact(DisplayName = "CursorPosition Updates When Cursor Is Repositioned Without Text Change (Issue 30779)")]
		public async Task CursorPositionUpdatesWhenRepositionedWithoutTextChange()
		{
			// Verifies: manually re-positioning the cursor updates CursorPosition (the gap fixed for iOS)
			var searchBar = new SearchBarStub { Text = "Hello World" };
			int cursorAfterReposition = -1;

			await AttachAndRun(searchBar, (handler) =>
			{
				var control = handler.QueryEditor;
				// Must be focused: OnSelectionChanged only updates cursor when IsFirstResponder.
				control.BecomeFirstResponder();

				// Move cursor to position 5 (between "Hello" and " World"), no text change
				var pos = control.GetPosition(control.BeginningOfDocument, 5);
				// SelectedTextRange assignment fires DidChangeSelection synchronously
				control.SelectedTextRange = control.GetTextRange(pos, pos);

				cursorAfterReposition = searchBar.CursorPosition;
			});

			// Cursor should be at position 5 (no text was typed, just repositioned)
			Assert.Equal(5, cursorAfterReposition);
		}

		[Fact(DisplayName = "SelectionLength And CursorPosition Both Update On Text Selection (Issue 30779)")]
		public async Task SelectionLengthAndCursorPositionBothUpdateOnSelection()
		{
			// Verifies: selection length gets the correct selected value (the gap fixed for iOS)
			var searchBar = new SearchBarStub { Text = "Hello World" };
			int cursorAfterSelect = -1;
			int selectionAfterSelect = -1;

			await AttachAndRun(searchBar, (handler) =>
			{
				var control = handler.QueryEditor;
				// Must be focused: OnSelectionChanged only updates cursor/selection when IsFirstResponder.
				control.BecomeFirstResponder();

				// Select " World" (6 chars starting at position 5)
				var start = control.GetPosition(control.BeginningOfDocument, 5);
				var end = control.GetPosition(control.BeginningOfDocument, 11);
				// SelectedTextRange assignment fires DidChangeSelection synchronously
				control.SelectedTextRange = control.GetTextRange(start, end);

				cursorAfterSelect = searchBar.CursorPosition;
				selectionAfterSelect = searchBar.SelectionLength;
			});

			Assert.Equal(5, cursorAfterSelect);
			Assert.Equal(6, selectionAfterSelect);
		}


		[Theory(DisplayName = "Gradient Background Initializes Correctly")]
		[InlineData(0xFFFF0000, 0xFFFE2500)]
		[InlineData(0xFF00FF00, 0xFF04F800)]
		[InlineData(0xFF0000FF, 0xFF0432FE)]
		public async Task GradientBackgroundInitializesCorrectly(uint colorToSet, uint expectedColor)
		{
			var color = Color.FromUint(colorToSet);
			var expected = Color.FromUint(expectedColor);

			var brush = new LinearGradientPaintStub(Colors.Black, color);

			var searchBar = new SearchBarStub
			{
				Background = brush,
				Height = 71,
				Width = 256,
				Text = "Background"
			};

			await ValidateHasColor(searchBar, expected, tolerance: .05);
		}

		[Fact(DisplayName = "ReturnType Initializes Correctly")]
		public async Task ReturnTypeInitializesCorrectly()
		{
			var xplatReturnType = ReturnType.Next;
			var entry = new SearchBarStub()
			{
				Text = "Test",
				ReturnType = xplatReturnType
			};

			UIReturnKeyType expectedValue = UIReturnKeyType.Next;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.ReturnType,
					PlatformViewValue = GetNativeReturnType(handler)
				};
			});

			Assert.Equal(xplatReturnType, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "MauiSearchBar disables InsetsLayoutMarginsFromSafeArea to prevent double safe-area inset (#34551)")]
		public async Task MauiSearchBarInsetsLayoutMarginsFromSafeAreaIsFalse()
		{
			var searchBar = new SearchBarStub();

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = CreateHandler(searchBar).PlatformView;
				var mauiSearchBar = Assert.IsType<MauiSearchBar>(platformView);
				Assert.False(mauiSearchBar.InsetsLayoutMarginsFromSafeArea);
			});
		}

		[Fact]
		public async Task ShouldShowCancelButtonToggles()
		{
			var searchBarStub = new SearchBarStub();

			await InvokeOnMainThreadAsync(() =>
			{
				var searchBar = CreateHandler(searchBarStub).PlatformView;

				Assert.False(searchBar.ShowsCancelButton);

				searchBarStub.Text = "additional text";
				searchBarStub.Handler.UpdateValue(nameof(ISearchBar.Text));
				Assert.True(searchBar.ShowsCancelButton);

				searchBarStub.Text = "";
				searchBarStub.Handler.UpdateValue(nameof(ISearchBar.Text));
				Assert.False(searchBar.ShowsCancelButton);
			});
		}

		[Fact(DisplayName = "Horizontal TextAlignment Updates Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			UITextAlignment expectedValue = UITextAlignment.Right;

			var values = await GetValueAsync(searchBarStub, (handler) =>
			{
				return new
				{
					ViewValue = searchBarStub.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Updates Correctly")]
		public async Task VerticalTextAlignmentInitializesCorrectly()
		{
			var xplatVerticalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				VerticalTextAlignment = xplatVerticalTextAlignment
			};

			UIControlContentVerticalAlignment expectedValue = UIControlContentVerticalAlignment.Bottom;

			var values = await GetValueAsync(searchBarStub, (handler) =>
			{
				return new
				{
					ViewValue = searchBarStub.VerticalTextAlignment,
					PlatformViewValue = GetNativeVerticalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatVerticalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Test";
			var xplatCharacterSpacing = 4;

			var searchBar = new SearchBarStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(searchBar, (handler) =>
			{
				return new
				{
					ViewValue = searchBar.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		double GetInputFieldHeight(SearchBarHandler searchBarHandler)
		{
			return GetNativeSearchBar(searchBarHandler).Bounds.Height;
		}

		static UISearchBar GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			(UISearchBar)searchBarHandler.PlatformView;

		Color GetNativeBackgroundColor(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).BarTintColor.ToColor();

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Text;

		static void SetNativeText(SearchBarHandler searchBarHandler, string text) =>
			GetNativeSearchBar(searchBarHandler).Text = text;

		UIReturnKeyType GetNativeReturnType(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).ReturnKeyType;

		static int GetCursorStartPosition(SearchBarHandler searchBarHandler)
		{
			var control = searchBarHandler.QueryEditor;
			return (int)control.GetOffsetFromPosition(control.BeginningOfDocument, control.SelectedTextRange.Start);
		}

		static void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position)
		{
			var control = searchBarHandler.QueryEditor;
			var endPosition = control.GetPosition(control.BeginningOfDocument, position);
			control.SelectedTextRange = control.GetTextRange(endPosition, endPosition);
		}

		Color GetNativeTextColor(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return Colors.Transparent;

			return textField.TextColor.ToColor();
		}

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Placeholder;

		UITextAlignment GetNativeHorizontalTextAlignment(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return UITextAlignment.Left;

			return textField.TextAlignment;
		}

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return UIControlContentVerticalAlignment.Center;

			return textField.VerticalAlignment;
		}

		double GetNativeCharacterSpacing(SearchBarHandler searchBarHandler)
		{
			var searchBar = GetNativeSearchBar(searchBarHandler);
			var textField = searchBar.FindDescendantView<UITextField>();

			return textField.AttributedText.GetCharacterSpacing();
		}

		double GetNativeUnscaledFontSize(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return -1;

			return textField.Font.PointSize;
		}

		bool GetNativeIsReadOnly(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);

			return !uiSearchBar.UserInteractionEnabled;
		}

		bool GetNativeIsNumericKeyboard(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return false;

			return textField.KeyboardType == UIKeyboardType.DecimalPad;
		}

		bool GetNativeIsEmailKeyboard(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return false;

			return textField.KeyboardType == UIKeyboardType.EmailAddress;
		}

		bool GetNativeIsTelephoneKeyboard(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return false;

			return textField.KeyboardType == UIKeyboardType.PhonePad;
		}

		bool GetNativeIsUrlKeyboard(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return false;

			return textField.KeyboardType == UIKeyboardType.Url;
		}

		bool GetNativeIsTextKeyboard(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return false;

			return textField.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				textField.AutocorrectionType == UITextAutocorrectionType.Yes &&
				textField.SpellCheckingType == UITextSpellCheckingType.Yes;
		}

		bool GetNativeIsChatKeyboard(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField is null)
				return false;

			return textField.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				textField.AutocorrectionType == UITextAutocorrectionType.Yes &&
				textField.SpellCheckingType == UITextSpellCheckingType.Yes;
		}
		bool GetNativeIsTextPredictionEnabled(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var textField = searchView.GetSearchTextField();

			if (textField is null)
				return false;

			return textField.AutocorrectionType == UITextAutocorrectionType.Yes;
		}

		bool GetNativeIsSpellCheckEnabled(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var textField = searchView.GetSearchTextField();

			if (textField is null)
				return false;

			return textField.SpellCheckingType == UITextSpellCheckingType.Yes;
		}
	}
}