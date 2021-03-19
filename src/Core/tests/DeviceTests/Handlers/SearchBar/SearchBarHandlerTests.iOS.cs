using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
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
					NativeViewValue = GetNativeTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.NativeViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Test";
			var xplatCharacterSpacing = 4;

			var slider = new SearchBarStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.CharacterSpacing,
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.NativeViewValue);
		}

		UISearchBar GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			(UISearchBar)searchBarHandler.View;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Text;

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Placeholder;

		UITextAlignment GetNativeTextAlignment(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return UITextAlignment.Left;

			return textField.TextAlignment;
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

			if (textField == null)
				return -1;

			return textField.Font.PointSize;
		}

		bool GetNativeIsBold(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return false;

			return textField.Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);
		}

		bool GetNativeIsItalic(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return false;

			return textField.Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);
		}
	}
}