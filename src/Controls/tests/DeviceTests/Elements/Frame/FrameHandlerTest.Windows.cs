using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameHandlerTest
	{
		[Fact(DisplayName = "Clip Initializes ContainerView Correctly", Skip = "REVIEW FOR .NET8")]
		public override Task ContainerViewInitializesCorrectly()
		{
			return Task.CompletedTask;
		}

		[Fact(DisplayName = "ContainerView Remains If Shadow Mapper Runs Again", Skip = "REVIEW FOR .NET8")]
		public override Task ContainerViewRemainsIfShadowMapperRunsAgain()
		{
			return Task.CompletedTask;
		}

		[Fact(DisplayName = "ContainerView Adds And Removes", Skip = "REVIEW FOR .NET8")]
		public override Task ContainerViewAddsAndRemoves()
		{
			return Task.CompletedTask;
		}
	}
}
