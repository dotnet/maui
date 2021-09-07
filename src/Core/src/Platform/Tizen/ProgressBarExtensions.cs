using ElmSharp;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Value = progress.Progress;
		}

		public static void UpdateProgressColor(this ProgressBar nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Color = progress.ProgressColor.ToNativeEFL();
		}
	}
}