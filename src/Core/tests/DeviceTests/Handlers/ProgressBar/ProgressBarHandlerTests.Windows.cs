using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		ProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			progressBarHandler.PlatformView;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			GetNativeProgressBar(progressBarHandler).Value;

		async Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null)
		{
			var expected = await GetValueAsync(progressBar, handler =>
			{
				var native = GetNativeProgressBar(handler);
				action?.Invoke();

				var foreground = native.Foreground;

				if (foreground is SolidColorBrush solidColorBrush)
					return solidColorBrush.Color.ToColor();

				return null;
			});
			Assert.Equal(expected, color);
		}

		[Fact(DisplayName = "Null Progress Color Resets To Default")]
		public async Task NullProgressColorResetsToDefault()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.9,
				ProgressColor = Colors.Red,
			};

			// The foreground of a ProgressBar that never had a color set.
			var defaultColor = await GetValueAsync(new ProgressBarStub(), handler =>
			{
				var foreground = GetNativeProgressBar(handler).Foreground;
				return foreground is SolidColorBrush solidColorBrush ? solidColorBrush.Color.ToColor() : null;
			});

			await ValidateNativeProgressColor(progressBar, defaultColor, () => progressBar.ProgressColor = null);
		}
	}
}