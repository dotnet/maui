using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	// TODO: only iOS is working with the search bar focus tests
#if IOS || MACCATALYST
	[Category(TestCategory.SearchBar)]
	public class SearchBarFocusTests : FocusHandlerTests<SearchBarHandler, SearchBarStub, VerticalStackLayoutStub>
	{
		public SearchBarFocusTests()
		{
		}
	}
#endif
}
