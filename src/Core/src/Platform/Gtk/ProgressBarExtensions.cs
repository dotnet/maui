using Gtk;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this ProgressBar nativeProgressBar, IProgress progress)
		{
			// https://developer.gnome.org/gtk3/stable/GtkProgressBar.html#gtk-progress-bar-set-fraction
			nativeProgressBar.Fraction = progress.Progress;
			nativeProgressBar.TooltipText = $"{progress.Progress * 100}%";
		}
	}
}