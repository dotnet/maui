using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar platformProgressBar, IProgress progress)
		{
			platformProgressBar.Progress = progress.Progress;
		}

		public static void UpdateProgressColor(this ProgressBar platformProgressBar, IProgress progress)
		{
			platformProgressBar.ProgressColor = progress.ProgressColor.ToPlatform();
		}
	}
}