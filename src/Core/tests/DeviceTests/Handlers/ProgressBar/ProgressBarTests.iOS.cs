using UIKit;
using System.Threading.Tasks;
using Xunit;
using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		UIProgressView GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			(UIProgressView)progressBarHandler.View;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			GetNativeProgressBar(progressBarHandler).Progress;

		async Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null)
		{
			var expected = await GetValueAsync(progressBar, handler =>
			{
				var native = GetNativeProgressBar(handler);
				action?.Invoke();
				return native.ProgressTintColor.ToColor();
			});
			Assert.Equal(expected, color);
		}
	}
}