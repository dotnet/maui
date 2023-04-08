using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SearchBar)]
	public class SearchBarTextInputTests : TextInputHandlerTests<SearchBarHandler, SearchBarStub>
	{
		protected override void SetNativeText(SearchBarHandler searchBarHandler, string text)
		{
			SearchBarHandlerTests.SetNativeText(searchBarHandler, text);
		}

		protected override int GetCursorStartPosition(SearchBarHandler searchBarHandler)
		{
			return SearchBarHandlerTests.GetCursorStartPosition(searchBarHandler);
		}

		protected override void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position)
		{
			SearchBarHandlerTests.UpdateCursorStartPosition(searchBarHandler, position);
		}
	}
}
