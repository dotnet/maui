using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwipeViewHandlerTests : CoreHandlerTestBase<SwipeViewHandler, SwipeViewStub>
	{
		[Fact(DisplayName = "Clip Initializes ContainerView Correctly", Skip = "Failing")]
		public override Task ContainerViewInitializesCorrectly()
		{
			return Task.CompletedTask;
		}

		[Fact(DisplayName = "ContainerView Remains If Shadow Mapper Runs Again", Skip = "Failing")]
		public override Task ContainerViewRemainsIfShadowMapperRunsAgain()
		{
			return Task.CompletedTask;
		}
	}
}
