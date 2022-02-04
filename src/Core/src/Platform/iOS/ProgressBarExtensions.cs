using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this UIProgressView nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Progress = (float)progress.Progress;
		}

		public static void UpdateProgressColor(this UIProgressView nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.ProgressTintColor = progress.ProgressColor?.ToPlatform();
		}
	}
}