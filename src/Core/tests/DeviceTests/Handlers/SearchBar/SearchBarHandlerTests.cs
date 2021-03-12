using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SearchBar)]
	public partial class SearchBarHandlerTests : HandlerTestBase<SearchBarHandler, SearchBarStub>
	{
		public SearchBarHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

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
						n = null; // native platforms may not support null text
					return n;
				},
				setValue,
				unsetValue);
		}
	}
}
