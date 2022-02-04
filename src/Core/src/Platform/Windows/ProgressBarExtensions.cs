using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Value = progress.Progress;
		}

		public static void UpdateProgressColor(this ProgressBar nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.UpdateProgressColor(progress, null);
		}		

		public static void UpdateProgressColor(this ProgressBar nativeProgressBar, IProgress progress, object? foregroundDefault)
		{
			Color progressColor = progress.ProgressColor;

			if (progressColor.IsDefault())
			{
				if (foregroundDefault != null)
					nativeProgressBar.RestoreForegroundCache(foregroundDefault);
			}
			else
			{
				nativeProgressBar.Foreground = progressColor.ToPlatform();
			}
		}
	}
}