using ElmSharp;

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
			platformProgressBar.Color = progress.ProgressColor.ToPlatformEFL();
		}
	}
}