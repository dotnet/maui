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

		SearchView GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			(SearchView)searchBarHandler.View;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Query;

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).QueryHint;

		Android.Views.TextAlignment GetNativeTextAlignment(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return Android.Views.TextAlignment.Inherit;

			return editText.TextAlignment;
		}
	}
}