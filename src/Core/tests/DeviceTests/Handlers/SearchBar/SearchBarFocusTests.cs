using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SearchBar)]
	public class SearchBarFocusTests : FocusHandlerTests<SearchBarHandler, SearchBarStub, VerticalStackLayoutStub>
	{
		public SearchBarFocusTests()
		{
		}
	}
}
