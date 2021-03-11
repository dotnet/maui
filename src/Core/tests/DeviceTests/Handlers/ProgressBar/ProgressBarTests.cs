using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category("ProgressBarHandler")]
	public partial class ProgressBarHandlerTests : HandlerTestBase<ProgressBarHandler>
	{
		public ProgressBarHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

		[Theory(DisplayName = "Progress Initializes Correctly")]
		[InlineData(0.25)]
		[InlineData(0.5)]
		[InlineData(0.75)]
		[InlineData(1.0)]
		public async Task ProgressInitializesCorrectly(double progress)
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = progress
			};

			var expected = progressBar.Progress;

			await ValidatePropertyInitValue(progressBar, () => progressBar.Progress, GetNativeProgress, progressBar.Progress);
		}
	}
}