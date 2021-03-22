using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			Android.Views.TextAlignment expectedValue = Android.Views.TextAlignment.ViewEnd;

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
			var xplatCharacterSpacing = 4;

			var searchBar = new SearchBarStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Test"
			};

			float expectedValue = searchBar.CharacterSpacing.ToEm();

			var values = await GetValueAsync(searchBar, (handler) =>
			{
				return new
				{
					ViewValue = searchBar.CharacterSpacing,
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue, EmCoefficientPrecision);
		}

		SearchView GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			(SearchView)searchBarHandler.View;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Query;

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).QueryHint;

		double GetNativeCharacterSpacing(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText != null)
			{
				return editText.LetterSpacing;
			}

			return -1;
		}

		Android.Views.TextAlignment GetNativeTextAlignment(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();
			return editText.TextAlignment;
		}

		double GetNativeUnscaledFontSize(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return -1;

			return editText.TextSize / editText.Resources.DisplayMetrics.Density;
		}

		bool GetNativeIsBold(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return false;

			return editText.Typeface.IsBold;
		}

		bool GetNativeIsItalic(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return false;

			return editText.Typeface.IsItalic;
		}
	}
}