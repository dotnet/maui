using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ProgressBar)]
	public partial class ProgressBarHandlerTests : CoreHandlerTestBase<ProgressBarHandler, ProgressBarStub>
	{
		[Theory(DisplayName = "Progress Initializes Correctly")]
		[InlineData(0.25)]
		[InlineData(0.5)]
		[InlineData(0.75)]
		[InlineData(1.0)]
		public async Task ProgressInitializesCorrectly(double progress)
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = progress,
			};

			var expected = progressBar.Progress;

			await ValidatePropertyInitValue(progressBar, () => progressBar.Progress, GetNativeProgress, progressBar.Progress);
		}

		[Theory(DisplayName = "Progress Color Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		public async Task ProgressColorInitializesCorrectly(string colorHex)
		{
			Color progressColor = Color.FromArgb(colorHex);

			var progressBar = new ProgressBarStub()
			{
				Progress = 0.9,
				ProgressColor = progressColor
			};

			await ValidateNativeProgressColor(progressBar, progressColor);
		}

		[Fact(DisplayName = "Null Progress Color Doesn't Crash")]
		public async Task NullProgressColorDoesntCrash()
		{
			var progressBar = new ProgressBarStub()
			{
				ProgressColor = null
			};

			await CreateHandlerAsync(progressBar);
		}
	}
}