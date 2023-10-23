using Gtk;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar nativeProgressBar, IProgress progress)
		{
			// https://docs.gtk.org/gtk3/method.ProgressBar.set_fraction.html
			nativeProgressBar.Fraction = progress.Progress;
			nativeProgressBar.TooltipText = $"{progress.Progress * 100}%";
		}
	}
}