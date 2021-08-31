using Gtk;

namespace Microsoft.Maui.Handlers
{
	// https://docs.gtk.org/gtk3/class.ProgressBar.html
	public partial class ProgressBarHandler : ViewHandler<IProgress, ProgressBar>
	{
		protected override ProgressBar CreateNativeView()
		{
			return new ProgressBar();
		}

		public static void MapProgress(ProgressBarHandler handler, IProgress progress)
		{
			handler.NativeView?.UpdateProgress(progress);
		}
	}
}