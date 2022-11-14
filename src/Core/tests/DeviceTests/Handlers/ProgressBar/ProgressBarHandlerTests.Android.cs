using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AProgressBar = Android.Widget.ProgressBar;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		AProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			progressBarHandler.PlatformView;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			(double)GetNativeProgressBar(progressBarHandler).Progress / ProgressBarExtensions.Maximum;

		Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null) =>
			 ValidateHasColor(progressBar, color, action);

		Task ValidateHasColor(IProgress progressBar, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformProgressBar = GetNativeProgressBar(CreateHandler(progressBar));
				action?.Invoke();
				platformProgressBar.AssertContainsColor(color);
			});
		}

		[Fact(DisplayName = "Control meets basic accessibility requirements")]
		[Category(TestCategory.Accessibility)]
		public async Task PlatformViewIsAccessible()
		{
			var view = new ProgressBarStub();
			await AssertPlatformViewIsAccessible(view);
		}
	}
}