using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		[Fact(DisplayName = "Null Progress Color Resets To Default")]
		public async Task NullProgressColorResetsToDefault()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.5
			};

			var handler = await CreateHandlerAsync(progressBar);

			var values = await InvokeOnMainThreadAsync(() =>
			{
				var platformProgressBar = GetNativeProgressBar(handler);
				var defaultForeground = platformProgressBar.Foreground;

				progressBar.ProgressColor = Colors.Red;
				handler.UpdateValue(nameof(IProgress.ProgressColor));
				var redForeground = platformProgressBar.Foreground;

				progressBar.ProgressColor = null;
				handler.UpdateValue(nameof(IProgress.ProgressColor));

				return (
					Default: defaultForeground,
					Red: redForeground,
					Reset: platformProgressBar.Foreground,
					ResetLocalValue: platformProgressBar.ReadLocalValue(Control.ForegroundProperty));
			});

			Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(values.Red).Color.ToColor());
			// The local value is cleared, so the default theme brush applies again.
			Assert.Equal(DependencyProperty.UnsetValue, values.ResetLocalValue);
			if (values.Default is SolidColorBrush defaultBrush && values.Reset is SolidColorBrush resetBrush)
				Assert.Equal(defaultBrush.Color, resetBrush.Color);
			else
				Assert.Same(values.Default, values.Reset);
		}

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
	}
}