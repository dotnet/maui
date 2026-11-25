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
				var redColor = (platformProgressBar.Foreground as SolidColorBrush)?.Color.ToColor();

				progressBar.ProgressColor = null;
				handler.UpdateValue(nameof(IProgress.ProgressColor));
				var resetForeground = platformProgressBar.Foreground;

				return (
					Red: redColor,
					ResetLocalValue: platformProgressBar.ReadLocalValue(Control.ForegroundProperty),
					ResetMatchesDefault: ReferenceEquals(defaultForeground, resetForeground) ||
						(defaultForeground is SolidColorBrush defaultBrush && resetForeground is SolidColorBrush resetBrush &&
							defaultBrush.Color == resetBrush.Color));
			});

			Assert.Equal(Colors.Red, values.Red);
			Assert.Equal(DependencyProperty.UnsetValue, values.ResetLocalValue);
			Assert.True(values.ResetMatchesDefault);
		}

		[Fact(DisplayName = "Null Progress Color Restores Native Foreground Binding")]
		public async Task NullProgressColorRestoresNativeForegroundBinding()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.5
			};

			var handler = await CreateHandlerAsync(progressBar);

			var colors = await InvokeOnMainThreadAsync(() =>
			{
				var platformProgressBar = GetNativeProgressBar(handler);
				var source = new Border { Background = new SolidColorBrush(Microsoft.UI.Colors.Green) };
				platformProgressBar.SetBinding(Control.ForegroundProperty, new Microsoft.UI.Xaml.Data.Binding
				{
					Source = source,
					Path = new PropertyPath(nameof(Border.Background))
				});

				progressBar.ProgressColor = Colors.Red;
				handler.UpdateValue(nameof(IProgress.ProgressColor));
				progressBar.ProgressColor = null;
				handler.UpdateValue(nameof(IProgress.ProgressColor));

				source.Background = new SolidColorBrush(Microsoft.UI.Colors.Blue);

				return (
					AfterSourceChange: (platformProgressBar.Foreground as SolidColorBrush)?.Color,
					IsBinding: platformProgressBar.ReadLocalValue(Control.ForegroundProperty) is Microsoft.UI.Xaml.Data.BindingExpression);
			});

			Assert.True(colors.IsBinding);
			Assert.Equal(Microsoft.UI.Colors.Blue, colors.AfterSourceChange);
		}

		[Fact(DisplayName = "Null Progress Color Keeps Native Foreground")]
		public async Task NullProgressColorKeepsNativeForeground()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.5
			};

			var handler = await CreateHandlerAsync(progressBar);

			var colors = await InvokeOnMainThreadAsync(() =>
			{
				var platformProgressBar = GetNativeProgressBar(handler);
				platformProgressBar.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);

				handler.UpdateValue(nameof(IProgress.ProgressColor));
				var afterNullMapping = (platformProgressBar.Foreground as SolidColorBrush)?.Color;

				progressBar.ProgressColor = Colors.Red;
				handler.UpdateValue(nameof(IProgress.ProgressColor));
				progressBar.ProgressColor = null;
				handler.UpdateValue(nameof(IProgress.ProgressColor));
				var afterReset = (platformProgressBar.Foreground as SolidColorBrush)?.Color;

				return (AfterNullMapping: afterNullMapping, AfterReset: afterReset);
			});

			Assert.Equal(Microsoft.UI.Colors.Green, colors.AfterNullMapping);
			Assert.Equal(Microsoft.UI.Colors.Green, colors.AfterReset);
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