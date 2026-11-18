using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar platformProgressBar, IProgress progress)
		{
			platformProgressBar.Value = progress.Progress;
		}

		public static void UpdateProgressColor(this ProgressBar platformProgressBar, IProgress progress)
		{
			Color progressColor = progress.ProgressColor;
			if (progressColor != null)
			{
				platformProgressBar.Foreground = progressColor.ToPlatform();
			}
			else
			{
				// Clear the local value so the ProgressBar falls back to its
				// default (theme) foreground instead of keeping the last color.
				platformProgressBar.ClearValue(ProgressBar.ForegroundProperty);
			}
		}
	}
}