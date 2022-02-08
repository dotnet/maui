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
			platformProgressBar.UpdateProgressColor(progress, null);
		}		

		public static void UpdateProgressColor(this ProgressBar platformProgressBar, IProgress progress, object? foregroundDefault)
		{
			Color progressColor = progress.ProgressColor;

			if (progressColor.IsDefault())
			{
				if (foregroundDefault != null)
					platformProgressBar.RestoreForegroundCache(foregroundDefault);
			}
			else
			{
				platformProgressBar.Foreground = progressColor.ToPlatform();
			}
		}
	}
}