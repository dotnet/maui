using Gtk;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar platformView, IProgress progress)
		{
			// https://docs.gtk.org/gtk3/method.ProgressBar.set_fraction.html
			platformView.Fraction = progress.Progress;
			platformView.TooltipText = $"{progress.Progress * 100}%";
		}
	}
}