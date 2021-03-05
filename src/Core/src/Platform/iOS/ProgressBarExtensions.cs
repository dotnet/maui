using UIKit;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this UIProgressView nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Progress = (float)progress.Progress;
		}
	}
}