using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
#if !WINDOWS
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewHandlerTests : CoreHandlerTestBase<SwipeViewHandler, SwipeViewStub>
	{
	}
#endif
}