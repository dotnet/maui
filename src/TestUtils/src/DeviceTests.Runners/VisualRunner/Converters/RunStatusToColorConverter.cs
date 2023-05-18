#nullable enable
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class RunStatusToColorConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is not RunStatus status || Application.Current == null)
				return Colors.Red;

			return status switch
			{
				RunStatus.Ok => Application.Current.Resources["VisualRunnerSuccessfulTestsColor"],
				RunStatus.Failed => Application.Current.Resources["VisualRunnerFailedTestsColor"],
				RunStatus.NoTests => Application.Current.Resources["VisualRunnerNoTestsColor"],
				RunStatus.NotRun => Application.Current.Resources["VisualRunnerNotRunTestsColor"],
				RunStatus.Skipped => Application.Current.Resources["VisualRunnerSkippedTestsColor"],
				_ => throw new ArgumentOutOfRangeException(nameof(value)),
			};
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}