using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SearchBar)]
	public partial class SearchBarHandlerTests : HandlerTestBase<SearchBarHandler, SearchBarStub>
	{
		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var searchBar = new SearchBarStub
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.Text, GetNativeText, searchBar.Text);
		}

		[Theory(DisplayName = "Query Text Updates Correctly")]
		[InlineData(null, null)]
		[InlineData(null, "Query")]
		[InlineData("Query", null)]
		[InlineData("Query", "Another search query")]
		public async Task TextUpdatesCorrectly(string setValue, string unsetValue)
		{
			var searchBar = new SearchBarStub();

			await ValidatePropertyUpdatesValue(
				searchBar,
				nameof(ISearchBar.Text),
				h =>
				{
					var n = GetNativeText(h);
					if (string.IsNullOrEmpty(n))
						n = null; // Native platforms may not support null text
					return n;
				},
				setValue,
				unsetValue);
		}

		[Fact(DisplayName = "Placeholder Initializes Correctly")]
		public async Task PlaceholderInitializesCorrectly()
		{
			var searchBar = new SearchBarStub
			{
				Placeholder = "Placeholder"
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.Placeholder, GetNativePlaceholder, searchBar.Placeholder);
		}

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var searchBar = new SearchBarStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.Font.FontSize, GetNativeUnscaledFontSize, searchBar.Font.FontSize);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontWeight.Regular, false, false)]
		[InlineData(FontWeight.Bold, true, false)]
		[InlineData(FontWeight.Regular, false, true)]
		[InlineData(FontWeight.Bold, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontWeight weight, bool isBold, bool isItalic)
		{
			var searchBar = new SearchBarStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", 10, weight, isItalic ? FontSlant.Italic : FontSlant.Default),
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.Font.Weight == FontWeight.Bold, GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(searchBar, () => searchBar.Font.FontSlant == FontSlant.Italic, GetNativeIsItalic, isItalic);
		}
	}
}