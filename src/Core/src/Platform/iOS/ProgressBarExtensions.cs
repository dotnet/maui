using UIKit;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this UIProgressView nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Progress = (float)progress.Progress;
		}

		public static void UpdateProgressColor(this UIProgressView nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.ProgressTintColor = progress.ProgressColor == Color.Default ? null : progress.ProgressColor.ToNative();
		}
	}
}