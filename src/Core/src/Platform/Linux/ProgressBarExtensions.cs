using Gtk;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.PulseStep = progress.Progress;
			nativeProgressBar.TooltipText = string.Format("{0}%", progress.Progress * 100);
		}
	}
}