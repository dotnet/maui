using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameHandlerTest
	{
		public override Task ContainerViewInitializesCorrectly()
		{
			// https://github.com/dotnet/maui/pull/12218
			return Task.CompletedTask;
		}

		public override Task ContainerViewAddsAndRemoves()
		{
			// https://github.com/dotnet/maui/pull/12218
			return Task.CompletedTask;
		}

		public override Task ContainerViewRemainsIfShadowMapperRunsAgain()
		{
			// https://github.com/dotnet/maui/pull/12218
			return Task.CompletedTask;
		}
	}
}
