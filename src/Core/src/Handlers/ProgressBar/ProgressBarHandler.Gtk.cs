using Gtk;

namespace Microsoft.Maui.Handlers
{
	// https://docs.gtk.org/gtk3/class.ProgressBar.html
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreatePlatformView()
		{
			return new ProgressBar();
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgress(progress);
		}

		[MissingMapper]
		public static void MapProgressColor(IProgressBarHandler handler, IProgress progress) { }

	}
}